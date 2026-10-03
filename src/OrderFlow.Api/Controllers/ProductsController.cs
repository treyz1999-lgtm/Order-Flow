using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private static readonly Product[] Products =
    [
        new(1, "Notebook", 4.99m, 25),
        new(2, "Pen", 1.50m, 100)
    ];

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        return Ok(Products);
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        Product? searchProduct = Products.FirstOrDefault(product => product.Id == id);

        if (searchProduct == null)
        {
            return NotFound();
        }

        return Ok(searchProduct);
    }
}
