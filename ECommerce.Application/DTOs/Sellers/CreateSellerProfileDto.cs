using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Selllers;

public class CreateSellerProfileDto
{
    [Required]
    [MaxLength(100)]
    public string StoreName { get; set; } = null!;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = null!;
}
