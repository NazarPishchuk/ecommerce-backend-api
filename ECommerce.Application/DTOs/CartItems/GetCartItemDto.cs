using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.CartItems;

public class GetCartItemDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public int AvailableQuantity { get; set; }

    public CartItemAvailabilityStatus AvailabilityStatus { get; set; }
}
