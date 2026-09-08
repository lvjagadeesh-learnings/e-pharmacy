using EPharmacy.Domain;
using Microsoft.EntityFrameworkCore;

namespace EPharmacy.Infrastructure;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<HealthCheck> HealthChecks => Set<HealthCheck>();

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<HealthCheck>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.CheckedAtUtc).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.DisplayName).IsRequired();
            entity.Property(u => u.CreatedAtUtc).IsRequired();
        });
    }
}
