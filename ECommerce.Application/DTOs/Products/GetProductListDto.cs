using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.Products;

public class GetProductListDto
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }
    public StockStatus StockStatus { get; set; }

    public string CategoryName { get; set; } = null!;
    public string StoreName { get; set; } = null!;
}
