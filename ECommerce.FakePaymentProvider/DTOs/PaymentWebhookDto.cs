namespace ECommerce.FakePaymentProvider.DTOs;

public record PaymentWebhookDto(
    string ProviderPaymentId,
    Guid PaymentId,
    string Status,
    DateTime? PaidAt);
