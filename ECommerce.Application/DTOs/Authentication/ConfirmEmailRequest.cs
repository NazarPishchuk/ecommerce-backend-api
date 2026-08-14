using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Authentication;

public sealed class ConfirmEmailRequest
{
    [Required]
    public required string UserId { get; init; }

    [Required]
    public required string Token { get; init; }
}