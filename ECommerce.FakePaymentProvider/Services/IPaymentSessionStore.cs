using ECommerce.FakePaymentProvider.Entities;

public interface IPaymentSessionStore
{
    void Add(PaymentSession session);

    PaymentSession? GetById(string id);
}
