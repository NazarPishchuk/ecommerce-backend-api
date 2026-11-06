using System.Text.Json;
using ECommerce.Contracts.Messaging;
using ECommerce.NotificationService.Email;
using ECommerce.NotificationService.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommerce.NotificationService.Messaging;

internal sealed class EmailConfirmationRequestedConsumer(
    IOptions<RabbitMqOptions> options,
    ILogger<EmailConfirmationRequestedConsumer> logger,
    IEmailSender emailSender,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            ClientProvidedName =
                "notification-service-email-consumer",

            AutomaticRecoveryEnabled = true,

            // Process callbacks sequentially in this service instance.
            ConsumerDispatchConcurrency = 1
        };

        await using var connection =
            await connectionFactory.CreateConnectionAsync(
                stoppingToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: _options.DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: _options.DeadLetterQueueName,
            exchange: _options.DeadLetterExchangeName,
            routingKey: MessageRoutingKeys.DeadLetterRoutingKey,
            cancellationToken: stoppingToken);

        var queueArguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] =
                _options.DeadLetterExchangeName,

            ["x-dead-letter-routing-key"] =
                MessageRoutingKeys.DeadLetterRoutingKey
        };

        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: _options.QueueName,
            exchange: _options.ExchangeName,
            routingKey:
                MessageRoutingKeys.EmailConfirmationRequested,
            cancellationToken: stoppingToken);

        // Receive one unacknowledged message at a time.
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                if (!Guid.TryParse(
                        delivery.BasicProperties.MessageId,
                        out var messageId) ||
                    messageId == Guid.Empty)
                {
                    logger.LogWarning(
                        "Notification rejected: missing or invalid MessageId.");

                    await channel.BasicNackAsync(
                        deliveryTag: delivery.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                await using var scope =
                    scopeFactory.CreateAsyncScope();

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<NotificationDbContext>();

                var alreadyProcessed =
                    await dbContext.ProcessedNotifications
                        .AnyAsync(
                            notification =>
                                notification.MessageId == messageId,
                            stoppingToken);

                if (alreadyProcessed)
                {
                    logger.LogInformation(
                        "Notification {MessageId} was already processed. " +
                        "Skipping email delivery.",
                        messageId);

                    await channel.BasicAckAsync(
                        deliveryTag: delivery.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                var body = delivery.Body.ToArray();

                var message =
                    JsonSerializer.Deserialize<EmailConfirmationRequested>(
                        body);

                if (message is null ||
                    string.IsNullOrWhiteSpace(message.UserId) ||
                    string.IsNullOrWhiteSpace(message.Email) ||
                    string.IsNullOrWhiteSpace(message.ConfirmationToken))
                {
                    logger.LogWarning(
                        "Notification {MessageId} has an invalid payload.",
                        messageId);

                    await channel.BasicNackAsync(
                        deliveryTag: delivery.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                await emailSender.SendConfirmationEmailAsync(
                    message.Email,
                    message.UserId,
                    message.ConfirmationToken,
                    stoppingToken);

                dbContext.ProcessedNotifications.Add(
                    new ProcessedNotification
                    {
                        MessageId = messageId,
                        ProcessedAtUtc = DateTimeOffset.UtcNow
                    });

                await dbContext.SaveChangesAsync(stoppingToken);

                logger.LogInformation(
                    "Notification {MessageId} was sent and recorded.",
                    messageId);

                await channel.BasicAckAsync(
                    deliveryTag: delivery.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Leave the message unacknowledged during shutdown.
                // Closing the connection allows RabbitMQ to redeliver it.
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to process notification {MessageId}.",
                    delivery.BasicProperties.MessageId);

                await channel.BasicNackAsync(
                    deliveryTag: delivery.DeliveryTag,
                    multiple: false,
                    requeue: false,
                    cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }
    }
}