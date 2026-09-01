using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders.Seller;

public class GetSellerOrderListDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public SellerOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
 