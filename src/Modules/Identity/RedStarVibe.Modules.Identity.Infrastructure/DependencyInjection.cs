using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RedStarVibe.BuildingBlocks.Persistence;
using RedStarVibe.Modules.Identity.Infrastructure.Persistence;

namespace RedStarVibe.Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    /// <param name="connectionString">Resolved when the DbContext is first created, so configuration can still be overridden (e.g. by tests).</param>
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services, Func<IServiceProvider, string> connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);

        // Scoped (one DbContext per request) by default: DbContext isn't thread-safe and is meant to be short-lived.
        services.AddDbContext<IdentityModuleDbContext>((sp, options) =>
            options.UseModuleSqlServer(connectionString(sp), IdentityModuleDbContext.Schema));

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
