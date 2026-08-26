using ECommerce.Application.DTOs.CartItems;

namespace ECommerce.Application.DTOs.Carts;

public class GetCartDto
{
    public IReadOnlyList<GetCartItemDto> Items { get; set; } = [];
    public int TotalItems { get; set; }
    public decimal TotalPrice { get; set; }
}
