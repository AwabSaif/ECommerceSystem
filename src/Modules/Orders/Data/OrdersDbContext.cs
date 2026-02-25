using Microsoft.EntityFrameworkCore;
using Modules.Orders.Entities;

namespace Modules.Orders.Data;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
    
        modelBuilder.Entity<Order>().ToTable("Orders_Transactions");
    }
}