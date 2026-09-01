using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders.Seller;

public class GetSellerOrderDetailsDto
{
    public int Id { get; set; }
    public SellerOrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public IReadOnlyList<GetOrderItemDto> OrderItems { get; set; }
        = new List<GetOrderItemDto>();
}
