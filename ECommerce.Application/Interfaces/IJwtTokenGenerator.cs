using ECommerce.Application.DTOs.Authentication;

namespace ECommerce.Application.Interfaces;

public interface IJwtTokenGenerator
{
    AccessTokenResponse GenerateToken(
        string userId,
        string email,
        IEnumerable<string> roles);
}
