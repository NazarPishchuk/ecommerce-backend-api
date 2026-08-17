namespace ECommerce.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; init; }
    public required string Type { get; init; }
    public required string RoutingKey { get; init; }
    public required string Payload { get; init; }
    public DateTimeOffset OccurredAtUtc { get; init; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
}
