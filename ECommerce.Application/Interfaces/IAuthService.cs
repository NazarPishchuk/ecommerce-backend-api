using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface IAuthService
{
    Task<Result<RegisteredUserResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AccessTokenResponse>> LoginAsync(LoginRequest request);
    Task<Result> ConfirmEmailAsync(string userId, string token);
}
