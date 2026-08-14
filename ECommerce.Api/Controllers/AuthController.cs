using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Route("api/auth")]
[ApiController]
public sealed class AuthController(IAuthService authService) : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result.Value);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request)
    {
        var result = await authService.ConfirmEmailAsync(request.UserId, request.Token);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }
}
