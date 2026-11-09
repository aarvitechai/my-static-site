using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedStarVibe.Modules.Identity.Domain;

namespace RedStarVibe.Modules.Identity.Infrastructure.Persistence.Configurations;

/// <summary>Seeds the platform roles. Seed data is part of the migration, so every environment gets identical rows.</summary>
internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    // Fixed IDs and concurrency stamps: seed data must be deterministic, otherwise EF Core would see
    // "changed" rows and generate a new migration every time.
    private static readonly Role[] SeedRoles =
    [
        Seed("0b6e7b1a-6a59-4f0a-9d0e-2f7c4c9a0001", Roles.User),
        Seed("0b6e7b1a-6a59-4f0a-9d0e-2f7c4c9a0002", Roles.Vendor),
        Seed("0b6e7b1a-6a59-4f0a-9d0e-2f7c4c9a0003", Roles.BusinessOwner),
        Seed("0b6e7b1a-6a59-4f0a-9d0e-2f7c4c9a0004", Roles.Moderator),
        Seed("0b6e7b1a-6a59-4f0a-9d0e-2f7c4c9a0005", Roles.Admin),
    ];

    public void Configure(EntityTypeBuilder<Role> builder) => builder.HasData(SeedRoles);

    private static Role Seed(string id, string name) => new()
    {
        Id = Guid.Parse(id),
        Name = name,
        NormalizedName = name.ToUpperInvariant(),
        ConcurrencyStamp = id,
    };
}
