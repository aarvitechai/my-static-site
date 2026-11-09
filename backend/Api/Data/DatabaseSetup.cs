using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public static class DatabaseSetup
{
    // Reads "Database:Provider" from appsettings and registers the matching DbContext.
    // Controllers and services just ask for AppDbContext and never know which database is used.
    public static IServiceCollection AddAppDatabase(this IServiceCollection services, IConfiguration config)
    {
        var provider = config["Database:Provider"] ?? "Sqlite";

        switch (provider)
        {
            case "SqlServer":
                services.AddDbContext<AppDbContext, SqlServerDbContext>(o =>
                    o.UseSqlServer(config.GetConnectionString("SqlServer")));
                break;
            case "Sqlite":
                services.AddDbContext<AppDbContext, SqliteDbContext>(o =>
                    o.UseSqlite(config.GetConnectionString("Sqlite")));
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown Database:Provider '{provider}'. Use 'SqlServer' or 'Sqlite'.");
        }

        return services;
    }
}
