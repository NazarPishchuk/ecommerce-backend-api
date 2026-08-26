using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.CartItems;

public class GetCartItemDto
{
    public int Id { get; set; } //auto

    public int ProductId { get; set; } //auto

    public string ProductName { get; set; } = null!; //select from np

    public decimal Price { get; set; } //selecrt from np

    public int Quantity { get; set; } //auto

    public int AvailableQuantity { get; set; } // select from np

    public CartItemAvailabilityStatus AvailabilityStatus { get; set; } //wil be defined in service
}
