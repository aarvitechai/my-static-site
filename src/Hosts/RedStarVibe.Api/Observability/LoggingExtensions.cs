using Serilog;
using Serilog.Events;

namespace RedStarVibe.Api.Observability;

internal static class LoggingExtensions
{
    /// <summary>
    /// Structured logging with Serilog. Sinks and levels come from the "Serilog" config section,
    /// so Development can log to Seq while other environments choose their own targets without code changes.
    /// </summary>
    public static WebApplicationBuilder AddApiLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddSerilog((services, logger) => logger
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()   // picks up CorrelationId pushed by CorrelationIdMiddleware
            .Enrich.WithProperty("Application", "RedStarVibe.Api")
            .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName));

        return builder;
    }

    /// <summary>One summary log line per request: method, path, status code, duration, correlation ID.</summary>
    public static IApplicationBuilder UseApiRequestLogging(this IApplicationBuilder app) =>
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0} ms";

            // Health probes hit the API every few seconds; keep them out of normal logs unless they fail.
            options.GetLevel = (httpContext, _, exception) =>
                exception is not null || httpContext.Response.StatusCode >= 500 ? LogEventLevel.Error
                : httpContext.Request.Path.StartsWithSegments("/health") ? LogEventLevel.Verbose
                : LogEventLevel.Information;

            // Deliberately NOT logged: query strings, headers, bodies, IP addresses (may hold personal data or secrets).
        });
}
