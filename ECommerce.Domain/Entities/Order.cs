namespace ECommerce.Domain.Entities;

public class Order
{
    public int Id { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public string UserId { get; set; } = null!;
}
