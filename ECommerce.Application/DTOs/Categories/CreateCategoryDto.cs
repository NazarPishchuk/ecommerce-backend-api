using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Categories;

public sealed class CreateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;
}