using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Services;
using OrderFlow.Api.ViewModels;

namespace OrderFlow.Api.Controllers;

[Route("products")]
public class WebProductsController(ProductService products) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await products.GetProductsAsync(cancellationToken));

    [HttpGet("new")]
    public IActionResult Create() => View("Form", new ProductForm());

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("Form", form);
        await products.CreateProductAsync(new CreateProductRequest(form.Name, form.Price,
            form.StockQuantity), cancellationToken);
        TempData["Message"] = "Product added.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var product = await products.GetProductAsync(id, cancellationToken);
        return product is null ? NotFound() : View("Form", new ProductForm
        {
            Id = product.Id, Name = product.Name, Price = product.Price,
            StockQuantity = product.StockQuantity
        });
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductForm form, CancellationToken cancellationToken)
    {
        form.Id = id;
        if (!ModelState.IsValid) return View("Form", form);
        try
        {
            var updated = await products.UpdateProductAsync(id,
                new UpdateProductRequest(form.Name, form.Price, form.StockQuantity), cancellationToken);
            if (updated is null) return NotFound();
        }
        catch (DbUpdateConcurrencyException)
        {
            ModelState.AddModelError("", "This product changed. Reload the page and try again.");
            return View("Form", form);
        }
        TempData["Message"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            if (await products.DeleteProductAsync(id, cancellationToken))
                TempData["Message"] = "Product deleted.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "This product belongs to an order and cannot be deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
