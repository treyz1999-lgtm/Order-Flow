using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;

namespace OrderFlow.Tests;

public class OrderWorkflowTests
{
    [Fact]
    public async Task PlacingAndCancellingOrderAdjustsStockAndKeepsPriceSnapshot()
    {
        await WithDatabase(async db =>
        {
            var customer = new Customer { Name = "Buyer", Email = "buyer@example.invalid" };
            var product = new Product { Name = "Notebook", Price = 4.99m, StockQuantity = 5 };
            db.AddRange(customer, product);
            await db.SaveChangesAsync();
            var orders = new OrderService(db);

            var placed = await orders.CreateOrderAsync(new CreateOrderRequest(customer.Id,
                [new CreateOrderItemRequest(product.Id, 2)]), CancellationToken.None);

            Assert.Equal(OrderError.None, placed.Error);
            Assert.Equal(9.98m, placed.Order!.Total);
            Assert.Equal(3, (await db.Products.AsNoTracking().SingleAsync()).StockQuantity);

            var cancelled = await orders.UpdateStatusAsync(placed.Order.Id, OrderStatus.Cancelled, CancellationToken.None);

            Assert.Equal(OrderError.None, cancelled.Error);
            Assert.Equal(OrderStatus.Cancelled, cancelled.Order!.Status);
            Assert.Equal(5, (await db.Products.AsNoTracking().SingleAsync()).StockQuantity);
            Assert.Equal(4.99m, cancelled.Order.Items.Single().UnitPrice);
        });
    }

    [Fact]
    public async Task InsufficientStockRejectsEntireOrder()
    {
        await WithDatabase(async db =>
        {
            var customer = new Customer { Name = "Buyer", Email = "buyer@example.invalid" };
            var stocked = new Product { Name = "Pen", Price = 1.50m, StockQuantity = 10 };
            var shortStock = new Product { Name = "Book", Price = 8.00m, StockQuantity = 1 };
            db.AddRange(customer, stocked, shortStock);
            await db.SaveChangesAsync();

            var result = await new OrderService(db).CreateOrderAsync(new CreateOrderRequest(customer.Id,
                [new CreateOrderItemRequest(stocked.Id, 2), new CreateOrderItemRequest(shortStock.Id, 3)]),
                CancellationToken.None);

            Assert.Equal(OrderError.InsufficientStock, result.Error);
            Assert.Empty(await db.Orders.AsNoTracking().ToListAsync());
            Assert.Equal(10, (await db.Products.AsNoTracking().SingleAsync(p => p.Id == stocked.Id)).StockQuantity);
            Assert.Equal(1, (await db.Products.AsNoTracking().SingleAsync(p => p.Id == shortStock.Id)).StockQuantity);
        });
    }

    [Fact]
    public async Task SalesReportExcludesCancelledOrders()
    {
        await WithDatabase(async db =>
        {
            var customer = new Customer { Name = "Buyer", Email = "buyer@example.invalid" };
            var product = new Product { Name = "Notebook", Price = 5m, StockQuantity = 10 };
            db.AddRange(customer, product);
            await db.SaveChangesAsync();
            var orders = new OrderService(db);

            var kept = await orders.CreateOrderAsync(new CreateOrderRequest(customer.Id,
                [new CreateOrderItemRequest(product.Id, 2)]), CancellationToken.None);
            var cancelled = await orders.CreateOrderAsync(new CreateOrderRequest(customer.Id,
                [new CreateOrderItemRequest(product.Id, 1)]), CancellationToken.None);
            await orders.UpdateStatusAsync(cancelled.Order!.Id, OrderStatus.Cancelled, CancellationToken.None);

            var summary = await new ReportService(db).GetSalesSummaryAsync(CancellationToken.None);
            Assert.Equal(OrderError.None, kept.Error);
            Assert.Equal(1, summary.OrderCount);
            Assert.Equal(2, summary.UnitsOrdered);
            Assert.Equal(10m, summary.BookedRevenue);
        });
    }

    private static async Task WithDatabase(Func<OrderFlowDbContext, Task> assertion)
    {
        var databaseName = $"OrderFlowTest_{Guid.NewGuid():N}";
        var options = new DbContextOptionsBuilder<OrderFlowDbContext>()
            .UseSqlServer($"Server=(localdb)\\mssqllocaldb;Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options;
        await using var db = new OrderFlowDbContext(options);
        try
        {
            await db.Database.MigrateAsync();
            await assertion(db);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }
}
