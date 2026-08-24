using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Products;

public class GetSellerProductDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;

    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public bool IsActive { get; set; }
    public StockStatus StockStatus { get; set; }
    public bool IsBlocked { get; set; }

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
}
