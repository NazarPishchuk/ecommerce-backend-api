namespace ECommerce.FakePaymentProvider.Entities;

public class PaymentSession
{
    public string Id { get; init; } = $"fake_{Guid.NewGuid():N}";

    public Guid PaymentId { get; init; }

    public decimal Amount { get; init; }

    public string Currency { get; init; } = string.Empty;

    public FakePaymentStatus Status { get; set; } =
        FakePaymentStatus.Pending;

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? PaidAt { get; set; }
}
