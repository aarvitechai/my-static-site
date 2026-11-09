using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedStarVibe.IntegrationTests.Infrastructure;
using RedStarVibe.Modules.Identity.Domain;
using RedStarVibe.Modules.Identity.Infrastructure.Persistence;

namespace RedStarVibe.IntegrationTests;

/// <summary>Phase 1 Definition of Done: the real API starts against real SQL Server and Redis, and migrations apply to an empty database.</summary>
public sealed class ApiStartupTests(ContainersFixture containers)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task MigrationsApplyToAnEmptyDatabaseAndSeedRoles()
    {
        await using var api = new ApiFactory(containers.NewDatabaseConnectionString(), containers.RedisConnectionString);
        using var client = api.CreateClient();   // starting the app runs the Development migration step

        await using var sql = new SqlConnection(api.SqlConnectionString);
        await sql.OpenAsync(Ct);

        Assert.Equal(["20261109062712_0001_Init"], await QueryStringsAsync(sql, "SELECT MigrationId FROM [identity].[__EFMigrationsHistory]"));
        Assert.Equal(Roles.All.Order(), (await QueryStringsAsync(sql, "SELECT Name FROM [identity].[Roles]")).Order());
        Assert.Empty(await QueryStringsAsync(sql, "SELECT name FROM sys.tables WHERE SCHEMA_NAME(schema_id) = 'dbo'"));
    }

    [Fact]
    public async Task RestartingAgainstAMigratedDatabaseChangesNothing()
    {
        var database = containers.NewDatabaseConnectionString();
        await using (var first = new ApiFactory(database, containers.RedisConnectionString))
        {
            first.CreateClient().Dispose();
        }

        await using var second = new ApiFactory(database, containers.RedisConnectionString);
        using var client = second.CreateClient();

        await using var sql = new SqlConnection(database);
        await sql.OpenAsync(Ct);
        Assert.Single(await QueryStringsAsync(sql, "SELECT MigrationId FROM [identity].[__EFMigrationsHistory]"));
    }

    [Fact]
    public async Task ReadinessReportsSqlServerAndRedisHealthy()
    {
        await using var api = new ApiFactory(containers.NewDatabaseConnectionString(), containers.RedisConnectionString);
        using var client = api.CreateClient();

        var response = await client.GetAsync("/health/ready", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal("Healthy", body.RootElement.GetProperty("status").GetString());
        var checks = body.RootElement.GetProperty("checks").EnumerateArray()
            .ToDictionary(c => c.GetProperty("name").GetString()!, c => c.GetProperty("status").GetString());
        Assert.Equal("Healthy", checks["sqlserver"]);
        Assert.Equal("Healthy", checks["redis"]);
    }

    [Fact]
    public async Task PingRespondsWithCorrelationId()
    {
        await using var api = new ApiFactory(containers.NewDatabaseConnectionString(), containers.RedisConnectionString);
        using var client = api.CreateClient();

        var response = await client.GetAsync("/api/v1/ping", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
    }

    [Fact]
    public async Task DatabaseRejectsDuplicateEmailRegardlessOfCase()
    {
        await using var api = new ApiFactory(containers.NewDatabaseConnectionString(), containers.RedisConnectionString);
        api.CreateClient().Dispose();

        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityModuleDbContext>();
        db.Users.Add(NewUser("meena@example.com"));
        await db.SaveChangesAsync(Ct);

        db.Users.Add(NewUser("MEENA@example.com"));
        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync(Ct));
        Assert.Contains("UX_Users_NormalizedEmail", ex.InnerException?.Message, StringComparison.Ordinal);
    }

    private static User NewUser(string email) => new()
    {
        Id = Guid.NewGuid(),
        UserName = Guid.NewGuid().ToString("N"),
        NormalizedUserName = Guid.NewGuid().ToString("N").ToUpperInvariant(),
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        SecurityStamp = Guid.NewGuid().ToString(),
    };

    private static async Task<List<string>> QueryStringsAsync(SqlConnection connection, string sql)
    {
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(Ct);
        var values = new List<string>();
        while (await reader.ReadAsync(Ct))
        {
            values.Add(reader.GetString(0));
        }

        return values;
    }
}
