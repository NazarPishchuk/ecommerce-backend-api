using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Authentication;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
