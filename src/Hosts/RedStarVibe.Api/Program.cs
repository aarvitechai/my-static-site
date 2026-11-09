using System.Globalization;
using RedStarVibe.Api.Endpoints;
using RedStarVibe.Api.Errors;
using RedStarVibe.Api.Health;
using RedStarVibe.Api.Infrastructure;
using RedStarVibe.Api.Observability;
using RedStarVibe.Api.OpenApi;
using RedStarVibe.BuildingBlocks.Web;
using RedStarVibe.Modules.Identity.Endpoints;
using Serilog;

// Bootstrap logger: captures errors that happen before configuration is loaded (e.g. a bad appsettings file).
// It's replaced by the configured logger once the host starts.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)   // same log format on every machine
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Don't advertise the server software ("Server: Kestrel") to attackers.
    builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

    // ---------- Services ----------
    builder.AddApiLogging();
    builder.Services.AddApiErrorHandling();
    builder.Services.AddApiRedis();
    builder.Services.AddBuildingBlocksWeb(builder.Configuration);   // current user, rate limiting, idempotency
    builder.Services.AddApiHealthChecks();
    builder.Services.AddApiDocs();

    // ---------- Modules (ADR-0001) ----------
    builder.Services.AddIdentityModule();

    // TimeProvider is .NET's built-in, testable clock: inject it instead of calling DateTime.UtcNow,
    // so tests can control time. (plan.md calls this IClock.)
    builder.Services.AddSingleton(TimeProvider.System);

    var app = builder.Build();

    // Fail fast with a clear message if a connection string is missing (services read them lazily).
    app.EnsureRequiredConfiguration();

    // Module startup work (Development: apply pending migrations before taking traffic).
    await app.UseIdentityModuleAsync();

    // ---------- HTTP pipeline (order matters: each middleware wraps everything after it) ----------
    app.UseMiddleware<CorrelationIdMiddleware>();   // first, so every later log line carries the ID
    app.UseApiRequestLogging();                     // logs the final status, including errors handled below
    app.UseApiErrorHandling();                      // exceptions and empty 4xx/5xx -> ProblemDetails

    if (!app.Environment.IsDevelopment())
    {
        // Tell browsers to use HTTPS only for this host (HSTS). Not in Development, so localhost isn't pinned to HTTPS.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseBuildingBlocksWeb();                     // rate limiting, then idempotency keys

    // ---------- Endpoints ----------
    app.MapApiHealthChecks();   // /health/live, /health/ready (unversioned: they're for infrastructure, not clients)
    app.MapApiDocs();           // /openapi/v1.json, /scalar (Development only)

    // Every API endpoint lives under a versioned group, so breaking changes can later go to /api/v2
    // without affecting existing clients.
    var v1 = app.MapGroup("/api/v1");
    v1.MapPingEndpoints();

    await app.RunAsync();
    return 0;
}
catch (Exception ex) when (ex is not HostAbortedException)   // HostAbortedException is thrown on purpose by EF tooling
{
    Log.Fatal(ex, "RedStarVibe.Api terminated unexpectedly");
    return 1;
}
finally
{
    // Flush buffered log events (e.g. to Seq) before the process exits.
    await Log.CloseAndFlushAsync();
}

// Makes Program visible to integration tests (WebApplicationFactory<Program>) in slice 5.
public partial class Program;
