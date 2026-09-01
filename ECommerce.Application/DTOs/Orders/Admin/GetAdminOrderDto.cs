using ECommerce.Application.DTOs.Orders.Customer;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders.Admin;

public class GetAdminOrderDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public IReadOnlyList<GetSellerOrderDto> SellerOrders { get; set; }
        = new List<GetSellerOrderDto>();
}
