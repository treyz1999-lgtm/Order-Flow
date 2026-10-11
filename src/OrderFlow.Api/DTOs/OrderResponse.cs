using OrderFlow.Api.Models;

namespace OrderFlow.Api.DTOs;

public record OrderItemResponse(int ProductId, string ProductName, int Quantity, decimal UnitPrice, decimal LineTotal);

public record OrderResponse(int Id, int CustomerId, OrderStatus Status, DateTime CreatedAtUtc,
    List<OrderItemResponse> Items, decimal Total);
