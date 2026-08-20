using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Authentication;

public sealed class RegisterRequest
{
    [Required]
    [MaxLength(50)]
    public required string FirstName { get; init; }

    [Required]
    [MaxLength(50)]
    public required string LastName { get; init; }

    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public required string Email { get; init; }

    [Required]
    public required string Password { get; init; }
}
