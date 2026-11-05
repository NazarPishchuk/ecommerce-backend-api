namespace ECommerce.Infrastructure.Persistence.Inbox;

public sealed class InboxMessage
{
    public Guid Id { get; set; }

    public required string Type { get; set; }

    public DateTimeOffset ProcessedAtUtc { get; set; }
}