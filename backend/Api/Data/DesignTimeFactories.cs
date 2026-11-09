using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Api.Data;

// Used only by the `dotnet ef` CLI (migrations), never at runtime.
// They let you create migrations for either provider regardless of Database:Provider.

internal static class DesignTimeConfig
{
    public static IConfiguration Load() => new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile("appsettings.Development.json", optional: true)
        .Build();
}

public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
{
    public SqlServerDbContext CreateDbContext(string[] args) => new(
        new DbContextOptionsBuilder<SqlServerDbContext>()
            .UseSqlServer(DesignTimeConfig.Load().GetConnectionString("SqlServer"))
            .Options);
}

public class SqliteDbContextFactory : IDesignTimeDbContextFactory<SqliteDbContext>
{
    public SqliteDbContext CreateDbContext(string[] args) => new(
        new DbContextOptionsBuilder<SqliteDbContext>()
            .UseSqlite(DesignTimeConfig.Load().GetConnectionString("Sqlite"))
            .Options);
}
