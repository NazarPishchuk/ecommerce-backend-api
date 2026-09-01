using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders;

public class GetOrderDto
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public IReadOnlyList<GetSellerOrderDto> SellerOrders { get; set; }
        = new List<GetSellerOrderDto>();
}