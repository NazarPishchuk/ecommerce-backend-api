using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Products;

public class CreateProductDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = null!;

    [Range(0.01, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}


