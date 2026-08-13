using ECommerce.Application.Interfaces;
using System.Text.Json;

namespace ECommerce.Infrastructure.Persistence.Outbox;

public sealed class OutboxWriter(ECommerceDbContext dbContext) : IOutboxWriter
{
    public void Add<T>(T message)
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = typeof(T).Name,
            Payload = JsonSerializer.Serialize(message),
            OccurredAtUtc = DateTimeOffset.UtcNow
        };

        dbContext.OutboxMessages.Add(outboxMessage);
    }
}
