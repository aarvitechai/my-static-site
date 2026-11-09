using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

// The EF Core "unit of work": each DbSet<T> becomes a table.
// The app always works with AppDbContext; the provider-specific subclasses below
// only exist so SQL Server and SQLite can each have their own migrations.
public abstract class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Price).HasPrecision(18, 2);

            // Databases don't store DateTime "kind", so mark values read back as UTC
            // (otherwise JSON omits the trailing "Z" and browsers treat it as local time).
            entity.Property(p => p.CreatedAt).HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            // Seed data is inserted by the first migration.
            var seeded = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            entity.HasData(
                new Product { Id = 1, Name = "Keyboard", Price = 49.99m, CreatedAt = seeded },
                new Product { Id = 2, Name = "Mouse", Price = 19.99m, CreatedAt = seeded },
                new Product { Id = 3, Name = "Monitor", Price = 199.00m, CreatedAt = seeded });
        });
    }
}

public class SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : AppDbContext(options);

public class SqliteDbContext(DbContextOptions<SqliteDbContext> options) : AppDbContext(options);
