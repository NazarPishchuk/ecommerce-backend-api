namespace ECommerce.PaymentService.Providers;

public interface IPaymentProvider
{
    Task<PaymentProviderResult> CreatePaymentAsync(
        Guid paymentId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);
}
