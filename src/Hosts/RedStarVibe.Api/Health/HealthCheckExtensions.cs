using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RedStarVibe.Api.Health;

internal static class HealthCheckExtensions
{
    private const string ReadyTag = "ready";
    private static readonly TimeSpan CheckTimeout = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Liveness  (/health/live):  is the process running? No dependencies, so a database outage doesn't
    ///                            make the orchestrator restart healthy API instances.
    /// Readiness (/health/ready): can it serve traffic? Checks SQL Server and Redis; a load balancer stops
    ///                            sending requests to an instance that isn't ready.
    /// </summary>
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddSqlServer(RequiredConnectionString(configuration, "Sql"), name: "sqlserver", tags: [ReadyTag], timeout: CheckTimeout)
            .AddRedis(RequiredConnectionString(configuration, "Redis"), name: "redis", tags: [ReadyTag], timeout: CheckTimeout);

        return services;
    }

    public static IEndpointRouteBuilder MapApiHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,          // run no checks: answering at all proves the process is alive
            ResponseWriter = WriteResponse,
        });

        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadyTag),
            ResponseWriter = WriteResponse,
        });

        return endpoints;
    }

    // Fail at startup with a clear message instead of failing on the first request.
    private static string RequiredConnectionString(IConfiguration configuration, string name) =>
        configuration.GetConnectionString(name) is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException(
                $"Connection string '{name}' is not configured. See README: set it in appsettings.Development.json or with dotnet user-secrets.");

    // Names, statuses and timings only. Exception messages stay in the logs: they can expose
    // server names, ports or credentials to anyone who can reach the endpoint.
    private static Task WriteResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";
        var body = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 1),
            }),
        };
        return context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}
