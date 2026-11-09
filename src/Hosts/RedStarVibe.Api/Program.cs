using RedStarVibe.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Don't advertise the server software ("Server: Kestrel") to attackers.
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// TimeProvider is .NET's built-in, testable clock: inject it instead of calling DateTime.UtcNow,
// so tests can control time. (plan.md calls this IClock.)
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    // Tell browsers to use HTTPS only for this host (HSTS). Not in Development, so localhost isn't pinned to HTTPS.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Every endpoint lives under a versioned group, so breaking changes can later go to /api/v2
// without affecting existing clients.
var v1 = app.MapGroup("/api/v1");
v1.MapPingEndpoints();

app.Run();

// Makes Program visible to integration tests (WebApplicationFactory<Program>) in slice 5.
public partial class Program;
