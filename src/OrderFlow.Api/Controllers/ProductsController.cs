using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductService products) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetProducts(CancellationToken cancellationToken) =>
        Ok(await products.GetProductsAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetProduct(int id, CancellationToken cancellationToken)
    {
        var product = await products.GetProductAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await products.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await products.UpdateProductAsync(id, request, cancellationToken);
            return product is null ? NotFound() : Ok(product);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("The product changed while your update was being saved. Retry with fresh data.");
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        try
        {
            return await products.DeleteProductAsync(id, cancellationToken) ? NoContent() : NotFound();
        }
        catch (DbUpdateException)
        {
            return Conflict("Products referenced by orders cannot be deleted.");
        }
    }
}
