using System.ComponentModel.DataAnnotations;

namespace ECommerce.PaymentService.DTOs;

public class CreatePaymentDto
{
    [Range(1, int.MaxValue,
        ErrorMessage = "OrderId must be greater than zero.")]
    public int OrderId { get; init; }

    [Range(typeof(decimal), "0.01", "999999999.99",
        ParseLimitsInInvariantCulture = true,
        ErrorMessage = "Amount must be within the allowed range.")]
    public decimal Amount { get; init; }

    [Required]
    [RegularExpression(@"^[A-Za-z]{3}$",
        ErrorMessage = "Currency must contain exactly 3 letters.")]
    public string Currency { get; init; } = string.Empty;
}

