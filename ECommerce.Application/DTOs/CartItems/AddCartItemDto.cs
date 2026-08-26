using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.CartItems;

public class AddCartItemDto
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
