using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.Authentication
{
    public sealed class ResendConfirmationEmailRequest
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public required string Email { get; init; }
    }
}
