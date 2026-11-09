using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RedStarVibe.BuildingBlocks.Persistence;

namespace RedStarVibe.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// Used only by `dotnet ef` (design time), so migrations can be created without starting the whole API
/// and without needing its secrets. `migrations add` never connects; `database update` uses the env var.
/// </summary>
internal sealed class IdentityModuleDbContextFactory : IDesignTimeDbContextFactory<IdentityModuleDbContext>
{
    public IdentityModuleDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Sql")
            ?? "Server=localhost;Database=RedStarVibe;Trusted_Connection=True;TrustServerCertificate=True";

        var builder = new DbContextOptionsBuilder<IdentityModuleDbContext>();
        builder.UseModuleSqlServer(connectionString, IdentityModuleDbContext.Schema);

        return new IdentityModuleDbContext(builder.Options);
    }
}
