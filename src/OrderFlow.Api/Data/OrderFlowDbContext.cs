using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Data;

public class OrderFlowDbContext(DbContextOptions<OrderFlowDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2);
            entity.Property(p => p.RowVersion).IsRowVersion();
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Product_Price", "[Price] > 0");
                t.HasCheckConstraint("CK_Product_Stock", "[StockQuantity] >= 0");
            });
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(c => c.Email).IsUnique();
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(o => o.RowVersion).IsRowVersion();
            entity.HasOne(o => o.Customer).WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(i => i.ProductName).HasMaxLength(100).IsRequired();
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.HasOne(i => i.Product).WithMany().HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(i => new { i.OrderId, i.ProductId }).IsUnique();
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_OrderItem_Quantity", "[Quantity] > 0");
                t.HasCheckConstraint("CK_OrderItem_UnitPrice", "[UnitPrice] > 0");
            });
        });
    }
}
