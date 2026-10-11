using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Services;

public class ReportService(OrderFlowDbContext db)
{
    public async Task<List<ProductResponse>> GetLowStockAsync(int threshold, CancellationToken cancellationToken) =>
        await db.Products.AsNoTracking().Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity).ThenBy(p => p.Id)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.StockQuantity))
            .ToListAsync(cancellationToken);

    public async Task<SalesSummaryResponse> GetSalesSummaryAsync(CancellationToken cancellationToken)
    {
        var orderCount = await db.Orders.CountAsync(o => o.Status != OrderStatus.Cancelled, cancellationToken);
        var activeItems = db.OrderItems.Where(i => i.Order.Status != OrderStatus.Cancelled);
        var units = await activeItems.SumAsync(i => (int?)i.Quantity, cancellationToken) ?? 0;
        var bookedRevenue = await activeItems.SumAsync(i => (decimal?)(i.UnitPrice * i.Quantity), cancellationToken) ?? 0m;
        return new SalesSummaryResponse(orderCount, units, bookedRevenue);
    }
}
