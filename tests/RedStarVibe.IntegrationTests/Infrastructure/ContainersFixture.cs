using Microsoft.Data.SqlClient;
using RedStarVibe.IntegrationTests.Infrastructure;
using Testcontainers.MsSql;
using Testcontainers.Redis;

// Start the containers once for the whole test run (they take ~10-30 s), shared by every test class.
[assembly: AssemblyFixture(typeof(ContainersFixture))]

// Serilog's bootstrap logger is process-wide static state; running API hosts in parallel would race on it.
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]

namespace RedStarVibe.IntegrationTests.Infrastructure;

/// <summary>Disposable SQL Server and Redis, matching the versions used in development.</summary>
public sealed class ContainersFixture : IAsyncLifetime
{
    // Same major version as the local dev instance (SQL Server 2025) and docker-compose (Redis 7.4), pinned.
    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2025-CU10-ubuntu-22.04";
    private const string RedisImage = "redis:7.4-alpine";

    public MsSqlContainer Sql { get; } = new MsSqlBuilder(SqlServerImage).Build();

    public RedisContainer Redis { get; } = new RedisBuilder(RedisImage).Build();

    /// <summary>A connection string to a fresh, not-yet-existing database, so each test starts truly empty.</summary>
    public string NewDatabaseConnectionString() =>
        new SqlConnectionStringBuilder(Sql.GetConnectionString())
        {
            InitialCatalog = "RedStarVibe_IT_" + Guid.NewGuid().ToString("N")[..12],
        }.ConnectionString;

    public string RedisConnectionString => Redis.GetConnectionString() + ",abortConnect=false";

    public async ValueTask InitializeAsync() =>
        await Task.WhenAll(Sql.StartAsync(), Redis.StartAsync());

    public async ValueTask DisposeAsync()
    {
        await Sql.DisposeAsync();
        await Redis.DisposeAsync();
    }
}
