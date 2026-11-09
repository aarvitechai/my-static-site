using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedStarVibe.Modules.Identity.Domain;

namespace RedStarVibe.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // datetimeoffset keeps the UTC offset, so times are never ambiguous.
        builder.Property(u => u.CreatedAt)
            .HasColumnType("datetimeoffset(7)")
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Identity creates non-unique indexes on the normalized columns by default; email must be unique.
        builder.HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("UX_Users_NormalizedEmail")
            .IsUnique()
            .HasFilter("[NormalizedEmail] IS NOT NULL");
    }
}
