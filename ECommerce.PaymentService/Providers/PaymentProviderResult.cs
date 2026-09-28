namespace ECommerce.PaymentService.Providers;

public record PaymentProviderResult(
    string ProviderPaymentId,
    string PaymentUrl);
