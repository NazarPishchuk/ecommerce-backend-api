namespace ECommerce.PaymentService.Domain;

public class Payment
{
    public Guid Id { get; set; }

    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public PaymentStatus Status { get; set; }

    public string? ProviderPaymentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }
}
