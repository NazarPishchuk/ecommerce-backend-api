using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class SellerOrder
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int? SellerProfileId { get; set; }
    public SellerProfile? SellerProfile { get; set; }

    public string StoreName { get; set; } = null!;

    public SellerOrderStatus Status { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }
        = new List<OrderItem>();
}
