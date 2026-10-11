using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.Data;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Services;

public class ProductService(OrderFlowDbContext db)
{
    public async Task<List<ProductResponse>> GetProductsAsync(CancellationToken cancellationToken) =>
        await db.Products.AsNoTracking().OrderBy(p => p.Id)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.StockQuantity))
            .ToListAsync(cancellationToken);

    public async Task<ProductResponse?> GetProductAsync(int id, CancellationToken cancellationToken) =>
        await db.Products.AsNoTracking().Where(p => p.Id == id)
            .Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.StockQuantity))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product { Name = request.Name.Trim(), Price = request.Price, StockQuantity = request.StockQuantity };
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return new ProductResponse(product.Id, product.Name, product.Price, product.StockQuantity);
    }

    public async Task<ProductResponse?> UpdateProductAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null) return null;
        product.Name = request.Name.Trim();
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        await db.SaveChangesAsync(cancellationToken);
        return new ProductResponse(product.Id, product.Name, product.Price, product.StockQuantity);
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        var product = await db.Products.FindAsync([id], cancellationToken);
        if (product is null) return false;
        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
