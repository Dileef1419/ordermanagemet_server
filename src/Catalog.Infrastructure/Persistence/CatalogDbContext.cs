using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext
{
    public DbSet<Product> Products => Set<Product>();

    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("cat");

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Sku).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
        });
    }
}

public class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CategoryId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "INR";
    public int Available { get; set; }
}