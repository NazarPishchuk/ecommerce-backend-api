using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Order
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public OrderStatus Status { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? PaymentId { get; set; }

    public DateTimeOffset? PaidAtUtc { get; set; }

    public ICollection<SellerOrder> SellerOrders { get; set; }
        = new List<SellerOrder>();
}
