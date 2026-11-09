using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RedStarVibe.BuildingBlocks.Users;

namespace RedStarVibe.BuildingBlocks.Web.RateLimiting;

/// <summary>Limits from the "RateLimiting" config section (options pattern, validated at startup).</summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Requests allowed per client per window, across all endpoints.</summary>
    [Range(1, 100_000)]
    public int PermitLimit { get; set; } = 120;

    [Range(1, 3600)]
    public int WindowSeconds { get; set; } = 60;

    /// <summary>Requests per window for sensitive endpoints (login, register, password reset): see <see cref="RateLimitPolicies.Strict"/>.</summary>
    [Range(1, 1000)]
    public int StrictPermitLimit { get; set; } = 10;
}

public static class RateLimitPolicies
{
    /// <summary>Named policy for brute-force targets. Apply with <c>.RequireRateLimiting(RateLimitPolicies.Strict)</c>.</summary>
    public const string Strict = "strict";
}

public static class RateLimitingSetup
{
    /// <summary>
    /// Global fixed-window limit per client (signed-in user ID, else IP address) plus a stricter named policy.
    /// Protects against floods, scraping and password guessing. Health probes are never limited.
    /// </summary>
    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();   // bad config fails at startup, not on the first request

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            limiter.OnRejected = OnRejectedAsync;

            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(http =>
            {
                if (http.Request.Path.StartsWithSegments("/health"))
                {
                    return RateLimitPartition.GetNoLimiter("health");
                }

                var settings = http.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
                return RateLimitPartition.GetFixedWindowLimiter(ClientKey(http), _ => Window(settings.PermitLimit, settings.WindowSeconds));
            });

            limiter.AddPolicy(RateLimitPolicies.Strict, http =>
            {
                var settings = http.RequestServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
                return RateLimitPartition.GetFixedWindowLimiter("strict:" + ClientKey(http), _ => Window(settings.StrictPermitLimit, settings.WindowSeconds));
            });
        });

        return services;
    }

    // Behind a reverse proxy, RemoteIpAddress is the proxy's IP until forwarded headers are configured (Phase 17).
    private static string ClientKey(HttpContext http) =>
        http.RequestServices.GetRequiredService<ICurrentUser>().UserId is { } userId
            ? "user:" + userId.ToString("N")
            : "ip:" + (http.Connection.RemoteIpAddress?.ToString() ?? "unknown");

    private static FixedWindowRateLimiterOptions Window(int permits, int seconds) => new()
    {
        PermitLimit = permits,
        Window = TimeSpan.FromSeconds(seconds),
        QueueLimit = 0,             // reject immediately rather than holding connections open
        AutoReplenishment = true,
    };

    private static async ValueTask OnRejectedAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var http = context.HttpContext;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            http.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        var problemDetails = http.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetails.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests.",
                Detail = "You're sending requests too quickly. Wait a moment and try again.",
            },
        });
    }
}
