using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace RedStarVibe.BuildingBlocks.Persistence;

/// <summary>
/// The SQL Server settings every module's DbContext shares (ADR-0002: one database, one schema per module).
/// Keeping them here means a module can't forget retries or accidentally write its migrations history into dbo.
/// </summary>
public static class ModuleDbContextOptions
{
    /// <summary>Retries for transient failures (failover, throttling, brief network loss).</summary>
    public const int MaxRetryCount = 5;

    public static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(10);

    /// <summary>Fail a stuck query instead of holding a connection forever.</summary>
    public const int CommandTimeoutSeconds = 30;

    public static DbContextOptionsBuilder UseModuleSqlServer(
        this DbContextOptionsBuilder options,
        string connectionString,
        string schema)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        return options
            .UseSqlServer(connectionString, sql =>
            {
                // Each module tracks its own migrations inside its own schema, e.g. identity.__EFMigrationsHistory.
                sql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema);
                sql.EnableRetryOnFailure(MaxRetryCount, MaxRetryDelay, errorNumbersToAdd: null);
                sql.CommandTimeout(CommandTimeoutSeconds);
            })
            // Reads are the common case and don't need change tracking (faster, less memory).
            // Code that updates entities opts in with .AsTracking() or Attach/Update.
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}
