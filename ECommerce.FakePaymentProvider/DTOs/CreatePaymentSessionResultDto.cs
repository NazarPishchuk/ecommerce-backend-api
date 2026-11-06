namespace ECommerce.FakePaymentProvider.DTOs;

public record CreatePaymentSessionResultDto(
    string ProviderPaymentId,
    string PaymentUrl);

