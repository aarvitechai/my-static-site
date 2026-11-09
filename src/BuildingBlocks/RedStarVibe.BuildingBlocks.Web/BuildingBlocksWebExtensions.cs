using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RedStarVibe.BuildingBlocks.Users;
using RedStarVibe.BuildingBlocks.Web.Idempotency;
using RedStarVibe.BuildingBlocks.Web.RateLimiting;
using RedStarVibe.BuildingBlocks.Web.Users;

namespace RedStarVibe.BuildingBlocks.Web;

public static class BuildingBlocksWebExtensions
{
    /// <summary>
    /// Current user, rate limiting and idempotency. The idempotency store defaults to Redis and needs an
    /// IConnectionMultiplexer registered by the host; tests can register their own IIdempotencyStore first.
    /// </summary>
    public static IServiceCollection AddBuildingBlocksWeb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddApiRateLimiting(configuration);

        services.AddOptions<IdempotencyOptions>();
        services.TryAddSingleton<IIdempotencyStore, RedisIdempotencyStore>();
        return services;
    }

    /// <summary>Rate limiting first (cheap rejection), then idempotency, then the endpoint.</summary>
    public static IApplicationBuilder UseBuildingBlocksWeb(this IApplicationBuilder app)
    {
        app.UseRateLimiter();
        app.UseMiddleware<IdempotencyMiddleware>();
        return app;
    }
}
