using OrderFlow.Api.Services;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService inventoryService;

    public ProductsController(ProductService suppliedService)
    {
        inventoryService = suppliedService;

    }


    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        return Ok(inventoryService.GetProducts());
    }

    // Controller: returns an HTTP response
    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        Product? searchProduct = inventoryService.GetProduct(id);

        if (searchProduct == null)
        {
            return NotFound();
        }

        return Ok(searchProduct);
    }
}
