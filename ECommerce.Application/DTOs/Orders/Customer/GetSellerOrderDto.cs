using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders.Customer;

public class GetSellerOrderDto
{
    public int Id { get; set; }
    public int? SellerProfileId { get; set; }
    public string StoreName { get; set; } = null!;
    public SellerOrderStatus Status { get; set; }

    public IReadOnlyList<GetOrderItemDto> OrderItems { get; set; }
        = new List<GetOrderItemDto>();
}