namespace ECommerce.NotificationService.Persistence;

public sealed class ProcessedNotification
{
    public Guid MessageId { get; set; }

    public DateTimeOffset ProcessedAtUtc { get; set; }
}
