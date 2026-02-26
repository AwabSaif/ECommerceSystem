using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Entities;

namespace Modules.Catalog.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        
        modelBuilder.Entity<Product>().ToTable("Catalog_Products");
        modelBuilder.Entity<Category>().ToTable("Catalog_Categories");
    }
}