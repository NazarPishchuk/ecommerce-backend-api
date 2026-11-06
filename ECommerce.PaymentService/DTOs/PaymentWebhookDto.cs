namespace ECommerce.PaymentService.DTOs;

public record PaymentWebhookDto(
    string ProviderPaymentId,
    Guid PaymentId,
    string Status,
    DateTime? PaidAt);
