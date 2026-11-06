using Microsoft.EntityFrameworkCore;

namespace ECommerce.NotificationService.Persistence;

public sealed class NotificationDbContext(
    DbContextOptions<NotificationDbContext> options)
    : DbContext(options)
{
    public DbSet<ProcessedNotification> ProcessedNotifications =>
        Set<ProcessedNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var notification =
            modelBuilder.Entity<ProcessedNotification>();

        notification.ToTable("ProcessedNotifications");

        notification.HasKey(x => x.MessageId);

        notification.Property(x => x.MessageId)
            .ValueGeneratedNever();

        notification.Property(x => x.ProcessedAtUtc)
            .IsRequired();
    }
}