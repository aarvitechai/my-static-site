using System.Text.RegularExpressions;
using Serilog.Context;

namespace RedStarVibe.Api.Observability;

/// <summary>
/// Gives every request a correlation ID so one request can be followed through all log lines,
/// the response header and any error body. A caller (another service, the SPA, a load balancer)
/// may send its own ID in <c>X-Correlation-ID</c>; otherwise a new one is generated.
/// </summary>
internal sealed partial class CorrelationIdMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-Correlation-ID";

    // Only accept short, plain IDs from the client. Anything else is replaced, so attackers can't
    // inject newlines or huge values into our logs (log forging) or reflect them back in responses.
    [GeneratedRegex("^[A-Za-z0-9._-]{1,64}$", RegexOptions.CultureInvariant)]
    private static partial Regex SafeId();

    public async Task InvokeAsync(HttpContext context)
    {
        var incoming = context.Request.Headers[HeaderName].ToString();
        var correlationId = SafeId().IsMatch(incoming) ? incoming : Guid.NewGuid().ToString("N");

        // TraceIdentifier is what ASP.NET Core (and our ProblemDetails) use to identify the request.
        context.TraceIdentifier = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Every log line written while handling this request gets a CorrelationId property.
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
