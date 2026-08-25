namespace ECommerce.Application.DTOs.Sellers;

public class GetSellerProfileDto
{
    public int Id { get; set; }

    public string StoreName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
