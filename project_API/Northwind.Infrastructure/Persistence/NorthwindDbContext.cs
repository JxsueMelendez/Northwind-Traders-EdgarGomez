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

        // Mapeo básico para OrderDetails (Llave compuesta)
        modelBuilder.Entity<OrderDetail>()
            .HasKey(od => new { od.OrderId, od.ProductId });

        // Configuramos los nombres de las tablas para que coincidan con la DB original
        modelBuilder.Entity<OrderDetail>().ToTable("Order Details");

        // Configurar ForeignKey para Shipper (ShipVia)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Shipper)
            .WithMany()
            .HasForeignKey(o => o.ShipVia);
    }
}
