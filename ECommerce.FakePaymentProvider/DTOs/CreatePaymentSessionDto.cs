
using System.ComponentModel.DataAnnotations;

namespace ECommerce.FakePaymentProvider.DTOs;

public record CreatePaymentSessionDto : IValidatableObject
{
    public Guid PaymentId { get; init; }

    [Range(typeof(decimal), "0.01", "999999999.99",
        ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; init; }

    [Required]
    [RegularExpression(@"^[A-Za-z]{3}$")]
    public string Currency { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (PaymentId == Guid.Empty)
        {
            yield return new ValidationResult(
                "PaymentId is required.",
                [nameof(PaymentId)]);
        }
    }
}

