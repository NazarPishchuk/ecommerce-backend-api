using ECommerce.PaymentService.Messaging;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.PaymentService.Persistence.Outbox;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    RabbitMqPublisher publisher,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private static readonly TimeSpan Interval =
        TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessMessagesAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                    when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Payment outbox processing failed.");
                }

                await Task.Delay(Interval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private async Task ProcessMessagesAsync(
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<PaymentDbContext>();

        var messages = await dbContext.OutboxMessages
            .AsNoTracking()
            .Where(message => message.ProcessedAtUtc == null)
            .OrderBy(message => message.OccurredAtUtc)
            .ThenBy(message => message.Id)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(
                    message,
                    cancellationToken);

                var processedAtUtc = DateTimeOffset.UtcNow;

                await dbContext.OutboxMessages
                    .Where(item =>
                        item.Id == message.Id &&
                        item.ProcessedAtUtc == null)
                    .ExecuteUpdateAsync(
                        setters => setters.SetProperty(
                            item => item.ProcessedAtUtc,
                            (DateTimeOffset?)processedAtUtc),
                        cancellationToken);

                logger.LogInformation(
                    "Published payment event {MessageId} with routing key {RoutingKey}.",
                    message.Id,
                    message.RoutingKey);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to publish or mark payment event {MessageId}. It will be retried.",
                    message.Id);

                break;
            }
        }
    }
}
