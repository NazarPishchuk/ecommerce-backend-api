using ECommerce.Application.DTOs.Authentication;

namespace ECommerce.Application.Interfaces;

public interface IJwtTokenGenerator
{
    LoginResponse GenerateToken(
        string userId,
        string email,
        IEnumerable<string> roles);
}
