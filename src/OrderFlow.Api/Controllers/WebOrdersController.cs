using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;
using OrderFlow.Api.ViewModels;

namespace OrderFlow.Api.Controllers;

[Route("orders")]
public class WebOrdersController(OrderService orders, CustomerService customers,
    ProductService products) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await orders.GetOrdersAsync(cancellationToken));

    [HttpGet("new")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken) =>
        View(await FillOptions(new OrderForm(), cancellationToken));

    [HttpPost("new")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(await FillOptions(form, cancellationToken));
        var result = await orders.CreateOrderAsync(new CreateOrderRequest(form.CustomerId,
            [new CreateOrderItemRequest(form.ProductId, form.Quantity)]), cancellationToken);
        if (result.Error != OrderError.None)
        {
            var message = result.Error switch
            {
                OrderError.CustomerNotFound => "Choose an existing customer.",
                OrderError.ProductNotFound => "Choose an existing product.",
                OrderError.InsufficientStock => "There is not enough stock for this order.",
                OrderError.Conflict => "Stock changed while placing the order. Try again.",
                _ => "Please check the order details."
            };
            ModelState.AddModelError("", message);
            return View(await FillOptions(form, cancellationToken));
        }
        TempData["Message"] = $"Order #{result.Order!.Id} placed. Stock was updated.";
        return RedirectToAction(nameof(Details), new { id = result.Order.Id });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var order = await orders.GetOrderAsync(id, cancellationToken);
        if (order is null) return NotFound();
        ViewBag.Customer = await customers.GetCustomerAsync(order.CustomerId, cancellationToken);
        return View(order);
    }

    [HttpPost("{id:int}/status")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, CancellationToken cancellationToken)
    {
        if (status is not (OrderStatus.Fulfilled or OrderStatus.Cancelled)) return BadRequest();
        var result = await orders.UpdateStatusAsync(id, status, cancellationToken);
        if (result.Error == OrderError.OrderNotFound) return NotFound();
        TempData[result.Error == OrderError.None ? "Message" : "Error"] = result.Error == OrderError.None
            ? $"Order #{id} marked {status}."
            : "Order could not be updated. Reload and try again.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<OrderForm> FillOptions(OrderForm form, CancellationToken cancellationToken)
    {
        form.Customers = (await customers.GetCustomersAsync(cancellationToken))
            .Select(c => new SelectListItem($"{c.Name} ({c.Email})", c.Id.ToString())).ToList();
        form.Products = (await products.GetProductsAsync(cancellationToken))
            .Select(p => new SelectListItem($"{p.Name} — {p.StockQuantity} in stock — {p.Price:C}",
                p.Id.ToString())).ToList();
        return form;
    }
}
