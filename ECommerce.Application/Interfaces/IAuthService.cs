using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result> ConfirmEmailAsync(string userId, string token);
    Task<Result> ResendEmailAsync(string email, CancellationToken cancellationToken);
}
