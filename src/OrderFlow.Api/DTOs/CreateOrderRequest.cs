using System.ComponentModel.DataAnnotations;
using OrderFlow.Api.Models;

namespace OrderFlow.Api.DTOs;

public record CreateOrderItemRequest(
    [Range(1, int.MaxValue)] int ProductId,
    [Range(1, int.MaxValue)] int Quantity);

public record CreateOrderRequest(
    [Range(1, int.MaxValue)] int CustomerId,
    [Required, MinLength(1)] List<CreateOrderItemRequest> Items);

public record UpdateOrderStatusRequest(OrderStatus Status);
