using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RedStarVibe.Modules.Identity.Domain;

namespace RedStarVibe.Modules.Identity.Infrastructure.Persistence;

/// <summary>
/// The Identity module's own DbContext. It maps only Identity tables, all inside the "identity" schema
/// (ADR-0002). Other modules never use it; they ask Identity through its contracts or events (ADR-0001).
/// </summary>
public sealed class IdentityModuleDbContext(DbContextOptions<IdentityModuleDbContext> options)
    : IdentityDbContext<User, Role, Guid>(options)
{
    public const string Schema = "identity";

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Identity's base mapping first, then our overrides.
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Schema);

        // Plain table names instead of the "AspNet" prefix: identity.Users, identity.Roles, ...
        builder.Entity<User>().ToTable("Users");
        builder.Entity<Role>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        // Picks up every IEntityTypeConfiguration<T> in this assembly (Configurations/ folder).
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityModuleDbContext).Assembly);
    }
}
