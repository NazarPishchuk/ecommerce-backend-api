using ECommerce.FakePaymentProvider.Entities;

public interface IPaymentSessionStore
{
    PaymentSession? GetOrCreate(
        Guid paymentId,
        decimal amount,
        string currency);

    PaymentSession? GetById(string id);
}
