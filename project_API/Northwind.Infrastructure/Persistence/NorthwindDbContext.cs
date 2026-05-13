using Microsoft.EntityFrameworkCore;
using Northwind.Domain.Entities;

namespace Northwind.Infrastructure.Persistence;

public class NorthwindDbContext : DbContext
{
    public NorthwindDbContext(DbContextOptions<NorthwindDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Shipper> Shippers => Set<Shipper>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Basic mapping for OrderDetails (Composite Key)
        modelBuilder.Entity<OrderDetail>()
            .HasKey(od => new { od.OrderId, od.ProductId });

        // Configure table names to match the original Northwind database schema
        modelBuilder.Entity<OrderDetail>().ToTable("Order Details");

        // Configure ForeignKey for Shipper (ShipVia property)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Shipper)
            .WithMany()
            .HasForeignKey(o => o.ShipVia);
    }
}
