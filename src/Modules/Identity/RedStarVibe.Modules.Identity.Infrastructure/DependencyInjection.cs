using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedStarVibe.BuildingBlocks.Persistence;
using RedStarVibe.Modules.Identity.Infrastructure.Persistence;

namespace RedStarVibe.Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Scoped (one DbContext per request) by default: DbContext isn't thread-safe and is meant to be short-lived.
        services.AddDbContext<IdentityModuleDbContext>(options =>
            options.UseModuleSqlServer(connectionString, IdentityModuleDbContext.Schema));

        return services;
    }

    /// <summary>Applies pending Identity migrations. The host calls this in Development only (see IdentityModule).</summary>
    public static async Task MigrateIdentityDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityModuleDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
