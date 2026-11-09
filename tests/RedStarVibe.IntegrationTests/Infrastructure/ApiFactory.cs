using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace RedStarVibe.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the real API (its actual Program.cs) in memory, pointed at the test containers.
/// Development environment, so migrations run at startup exactly as they do locally.
/// </summary>
public sealed class ApiFactory(string sqlConnectionString, string redisConnectionString) : WebApplicationFactory<Program>
{
    public string SqlConnectionString { get; } = sqlConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Added last, so it overrides appsettings.Development.json and any developer User Secrets.
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Sql"] = SqlConnectionString,
            ["ConnectionStrings:Redis"] = redisConnectionString,
            // Seq stays configured; with no Seq server (CI) its batches are simply dropped.
        }));
    }
}
