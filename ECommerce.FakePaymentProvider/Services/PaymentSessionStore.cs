
using ECommerce.FakePaymentProvider.Entities;

public class PaymentSessionStore : IPaymentSessionStore
{
    private readonly Dictionary<Guid, PaymentSession> _byPaymentId = new();
    private readonly Dictionary<string, PaymentSession> _bySessionId = new();
    private readonly object _gate = new();

    public PaymentSession? GetOrCreate(
        Guid paymentId,
        decimal amount,
        string currency)
    {
        lock (_gate)
        {
            if (_byPaymentId.TryGetValue(paymentId, out var existing))
            {
                if (existing.Amount != amount ||
                    existing.Currency != currency)
                {
                    return null;
                }

                return existing;
            }

            var session = new PaymentSession
            {
                PaymentId = paymentId,
                Amount = amount,
                Currency = currency
            };

            _byPaymentId.Add(paymentId, session);
            _bySessionId.Add(session.Id, session);

            return session;
        }
    }

    public PaymentSession? GetById(string id)
    {
        lock (_gate)
        {
            return _bySessionId.GetValueOrDefault(id);
        }
    }
}
