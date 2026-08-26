namespace ECommerce.Application.DTOs.Sellers;

public class GetAdminSellerProfileDto
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string StoreName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsBanned { get; set; }
}
