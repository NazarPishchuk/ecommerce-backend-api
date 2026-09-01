using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Orders.Customer;

public class GetOrderListDto
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
