namespace ECommerce.PaymentService.Providers;

public class FakePaymentProvider(
    HttpClient httpClient) : IPaymentProvider
{
    public async Task<PaymentProviderResult> CreatePaymentAsync(
        Guid paymentId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            PaymentId = paymentId,
            Amount = amount,
            Currency = currency
        };

        using var response = await httpClient.PostAsJsonAsync(
            "api/payments",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<PaymentProviderResult>(
                cancellationToken);

        if (result is null)
            throw new InvalidOperationException(
                "Payment provider returned an empty response.");

        return result;
    }
}

