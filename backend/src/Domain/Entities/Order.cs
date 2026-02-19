namespace ClietStockHub.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.CREATED;
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}
