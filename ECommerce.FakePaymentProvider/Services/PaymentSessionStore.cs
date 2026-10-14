using ECommerce.FakePaymentProvider.Entities;
using System.Collections.Concurrent;

public class PaymentSessionStore : IPaymentSessionStore
{
    private readonly ConcurrentDictionary<string, PaymentSession> _sessions = new();

    public void Add(PaymentSession session)
    {
        if (!_sessions.TryAdd(session.Id, session))
        {
            throw new InvalidOperationException(
                "Payment session already exists.");
        }
    }

    public PaymentSession? GetById(string id)
    {
        _sessions.TryGetValue(id, out var session);

        return session;
    }

}