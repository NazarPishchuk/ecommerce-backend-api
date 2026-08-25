using ECommerce.Application.DTOs.Authentication;

namespace ECommerce.Application.DTOs.Sellers;

public class CreateSellerProfileResponse
{
    public GetSellerProfileDto SellerProfile { get; set; } = null!;
    public LoginResponse Auth { get; set; } = null!;
}
