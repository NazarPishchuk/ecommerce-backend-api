using ECommerce.FakePaymentProvider.Entities;

namespace ECommerce.FakePaymentProvider.Services;

public interface IPaymentWebhookSender
{
    Task SendAsync(
        PaymentSession session,
        CancellationToken cancellationToken);
}
