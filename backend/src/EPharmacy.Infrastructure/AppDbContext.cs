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

    public DbSet<Medicine> Medicines => Set<Medicine>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<Order> Orders => Set<Order>();

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

        modelBuilder.Entity<Medicine>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.Name).IsRequired();
            entity.Property(m => m.Description).IsRequired();
            entity.Property(m => m.PriceCents).IsRequired();
            entity.Property(m => m.ImageUrl).IsRequired(false);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.UserId).IsRequired();
            entity.HasIndex(c => c.UserId).IsUnique();

            entity.OwnsMany(c => c.Items, itemsBuilder =>
            {
                itemsBuilder.WithOwner().HasForeignKey("CartId");
                itemsBuilder.Property(i => i.MedicineId).IsRequired();
                itemsBuilder.Property(i => i.Quantity).IsRequired();
                itemsBuilder.HasKey("CartId", "MedicineId");
            });

            entity.Navigation(c => c.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.UserId).IsRequired();
            entity.HasIndex(o => o.UserId);
            entity.Property(o => o.ShippingAddress).IsRequired();
            entity.Property(o => o.PlacedAtUtc).IsRequired();
            entity.Property(o => o.ReceivedAtUtc);
            entity.Ignore(o => o.TotalCents);

            entity.OwnsMany(o => o.Items, itemsBuilder =>
            {
                itemsBuilder.WithOwner().HasForeignKey("OrderId");
                itemsBuilder.Property(i => i.MedicineId).IsRequired();
                itemsBuilder.Property(i => i.Name).IsRequired();
                itemsBuilder.Property(i => i.UnitPriceCents).IsRequired();
                itemsBuilder.Property(i => i.Quantity).IsRequired();
                itemsBuilder.HasKey("OrderId", "MedicineId");
            });

            entity.Navigation(o => o.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
