using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RedStarVibe.Modules.Identity.Infrastructure;

namespace RedStarVibe.Modules.Identity.Endpoints;

/// <summary>
/// Everything the host needs to plug the Identity module in. Every module exposes the same shape
/// (Add...Module / Use...Module), so Program.cs reads like a list of modules.
/// </summary>
public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        services.AddIdentityInfrastructure(sp =>
            sp.GetRequiredService<IConfiguration>().GetConnectionString("Sql") is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException("Connection string 'Sql' is not configured."));
        return services;
    }

    /// <summary>
    /// Development: apply pending migrations at startup for a fast local loop.
    /// Other environments: migrations run as a separate, reviewed deployment step (plan.md Phase 17), never at app start,
    /// because the app's database login should not have permission to change the schema.
    /// </summary>
    public static async Task UseIdentityModuleAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            await app.Services.MigrateIdentityDatabaseAsync();
        }
    }
}
