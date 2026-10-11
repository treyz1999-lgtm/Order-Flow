using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Services;

public enum OrderError { None, InvalidItems, CustomerNotFound, ProductNotFound, InsufficientStock, OrderNotFound, InvalidStatus, Conflict }
public record OrderResult(OrderResponse? Order, OrderError Error);

public class OrderService(OrderFlowDbContext db)
{
    public async Task<List<OrderResponse>> GetOrdersAsync(CancellationToken cancellationToken)
    {
        var orders = await db.Orders.AsNoTracking().Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAtUtc).ToListAsync(cancellationToken);
        return orders.Select(ToResponse).ToList();
    }

    public async Task<OrderResponse?> GetOrderAsync(int id, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking().Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
        return order is null ? null : ToResponse(order);
    }

    public async Task<OrderResult> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0 ||
            request.Items.Any(i => i.ProductId <= 0 || i.Quantity <= 0) ||
            request.Items.Select(i => i.ProductId).Distinct().Count() != request.Items.Count)
            return new(null, OrderError.InvalidItems);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (!await db.Customers.AnyAsync(c => c.Id == request.CustomerId, cancellationToken))
            return new(null, OrderError.CustomerNotFound);

        var productIds = request.Items.Select(i => i.ProductId).ToArray();
        var products = await db.Products.Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);
        if (products.Count != productIds.Length) return new(null, OrderError.ProductNotFound);

        foreach (var item in request.Items)
        {
            if (products[item.ProductId].StockQuantity < item.Quantity)
                return new(null, OrderError.InsufficientStock);
        }

        var order = new Order { CustomerId = request.CustomerId, CreatedAtUtc = DateTime.UtcNow };
        foreach (var item in request.Items)
        {
            var product = products[item.ProductId];
            product.StockQuantity -= item.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id, ProductName = product.Name,
                Quantity = item.Quantity, UnitPrice = product.Price
            });
        }

        db.Orders.Add(order);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(ToResponse(order), OrderError.None);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(null, OrderError.Conflict);
        }
    }

    public async Task<OrderResult> UpdateStatusAsync(int id, OrderStatus status, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(status)) return new(null, OrderError.InvalidStatus);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var order = await db.Orders.Include(o => o.Items)
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order is null) return new(null, OrderError.OrderNotFound);
        if (order.Status == status) return new(ToResponse(order), OrderError.None);
        if (order.Status != OrderStatus.Placed || status == OrderStatus.Placed)
            return new(null, OrderError.InvalidStatus);

        if (status == OrderStatus.Cancelled)
        {
            var productIds = order.Items.Select(i => i.ProductId).ToArray();
            var products = await db.Products.Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);
            foreach (var item in order.Items)
                products[item.ProductId].StockQuantity = checked(products[item.ProductId].StockQuantity + item.Quantity);
        }
        order.Status = status;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(ToResponse(order), OrderError.None);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(null, OrderError.Conflict);
        }
    }

    private static OrderResponse ToResponse(Order order)
    {
        var items = order.Items.OrderBy(i => i.Id)
            .Select(i => new OrderItemResponse(i.ProductId, i.ProductName, i.Quantity,
                i.UnitPrice, i.UnitPrice * i.Quantity)).ToList();
        return new OrderResponse(order.Id, order.CustomerId, order.Status, order.CreatedAtUtc,
            items, items.Sum(i => i.LineTotal));
    }
}
