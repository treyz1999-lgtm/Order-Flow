using Microsoft.AspNetCore.Mvc;
using OrderFlow.Api.DTOs;
using OrderFlow.Api.Models;
using OrderFlow.Api.Services;

namespace OrderFlow.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(OrderService orders) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<OrderResponse>>> GetOrders(CancellationToken cancellationToken) =>
        Ok(await orders.GetOrdersAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderResponse>> GetOrder(int id, CancellationToken cancellationToken)
    {
        var order = await orders.GetOrderAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> CreateOrder(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await orders.CreateOrderAsync(request, cancellationToken);
        return result.Error switch
        {
            OrderError.None => CreatedAtAction(nameof(GetOrder), new { id = result.Order!.Id }, result.Order),
            OrderError.CustomerNotFound or OrderError.ProductNotFound => NotFound(result.Error.ToString()),
            OrderError.InsufficientStock or OrderError.Conflict => Conflict(result.Error.ToString()),
            _ => BadRequest("Provide distinct products with positive quantities.")
        };
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(int id, UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await orders.UpdateStatusAsync(id, request.Status, cancellationToken);
        return result.Error switch
        {
            OrderError.None => Ok(result.Order),
            OrderError.OrderNotFound => NotFound(),
            OrderError.Conflict => Conflict("Stock changed during cancellation. Retry."),
            _ => BadRequest("Only placed orders can be fulfilled or cancelled.")
        };
    }
}
