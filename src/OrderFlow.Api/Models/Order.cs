namespace OrderFlow.Api.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public OrderStatus Status { get; set; } = OrderStatus.Placed;
    public DateTime CreatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public List<OrderItem> Items { get; set; } = [];
}

public enum OrderStatus { Placed, Fulfilled, Cancelled }
