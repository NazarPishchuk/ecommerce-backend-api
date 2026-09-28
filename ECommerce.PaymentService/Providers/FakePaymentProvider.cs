namespace ECommerce.PaymentService.Providers;

public class FakePaymentProvider : IPaymentProvider
{
    public Task<PaymentProviderResult> CreatePaymentAsync(
        Guid paymentId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var providerPaymentId = Guid.NewGuid().ToString();

        var result = new PaymentProviderResult(
            providerPaymentId,
            $"https://fake-payment.local/pay/{providerPaymentId}");

        return Task.FromResult(result);
    }
}
