using ECommerce.Contracts.Messaging;
using ECommerce.NotificationService.Email;
using ECommerce.NotificationService.Persistence;
using MailKit;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text.Json;

namespace ECommerce.NotificationService.Messaging;

internal sealed class EmailConfirmationRequestedConsumer(
    IOptions<RabbitMqOptions> options,
    ILogger<EmailConfirmationRequestedConsumer> logger,
    IEmailSender emailSender,
    IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    private const string RetryHeader = "x-retry-count";

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
            ConsumerDispatchConcurrency = 1
        };

        await using var connection =
            await ConnectWithRetryAsync(
                connectionFactory,
                stoppingToken);

        // Confirm retry publications before ACKing originals.
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        await using var channel =
            await connection.CreateChannelAsync(
                options: channelOptions,
                cancellationToken: stoppingToken);

        // Main exchange
        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Final DLX
        await channel.ExchangeDeclareAsync(
            exchange: _options.DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Final DLQ
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

        // Main queue configuration
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

        // Retry Queue: 10 seconds
        await DeclareRetryQueueAsync(
            channel,
            _options.Retry10sQueueName,
            10000,
            stoppingToken);

        // Retry Queue: 30 seconds
        await DeclareRetryQueueAsync(
            channel,
            _options.Retry30sQueueName,
            30000,
            stoppingToken);

        // One unacknowledged message at a time
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var retryPublishFailure = new TaskCompletionSource<Exception>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                // 1. Validate MessageId
                if (!Guid.TryParse(
                        delivery.BasicProperties.MessageId,
                        out var messageId) ||
                    messageId == Guid.Empty)
                {
                    logger.LogWarning(
                        "Invalid notification MessageId.");

                    await channel.BasicNackAsync(
                        delivery.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                await using var scope =
                    scopeFactory.CreateAsyncScope();

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<NotificationDbContext>();

                // 2. Deduplication
                var alreadyProcessed =
                    await dbContext.ProcessedNotifications
                        .AnyAsync(
                            p => p.MessageId == messageId,
                            stoppingToken);

                if (alreadyProcessed)
                {
                    logger.LogInformation(
                        "Message {MessageId} already processed.",
                        messageId);

                    await channel.BasicAckAsync(
                        delivery.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                // 3. Deserialize
                EmailConfirmationRequested? message;

                try
                {
                    message =
                        JsonSerializer.Deserialize
                            <EmailConfirmationRequested>(
                                delivery.Body.ToArray());
                }
                catch (JsonException exception)
                {
                    logger.LogWarning(
                        exception,
                        "Invalid JSON in message {MessageId}.",
                        messageId);

                    await channel.BasicNackAsync(
                        delivery.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                if (message is null ||
                    string.IsNullOrWhiteSpace(message.UserId) ||
                    string.IsNullOrWhiteSpace(message.Email) ||
                    string.IsNullOrWhiteSpace(message.ConfirmationToken))
                {
                    logger.LogWarning(
                        "Invalid payload for {MessageId}.",
                        messageId);

                    await channel.BasicNackAsync(
                        delivery.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                // 4. Send email
                try
                {
                    await emailSender.SendConfirmationEmailAsync(
                        message.Email,
                        message.UserId,
                        message.ConfirmationToken,
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                    when (IsTemporaryEmailError(exception))
                {
                    logger.LogWarning(
                        exception,
                        "Temporary SMTP failure for {MessageId}.",
                        messageId);

                    await ScheduleRetryOrStopAsync(
                        channel,
                        delivery,
                        retryPublishFailure,
                        stoppingToken);

                    return;
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Permanent email failure for {MessageId}.",
                        messageId);

                    await channel.BasicNackAsync(
                        delivery.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);

                    return;
                }

                // 5. Record successful processing
                dbContext.ProcessedNotifications.Add(
                    new ProcessedNotification
                    {
                        MessageId = messageId,
                        ProcessedAtUtc = DateTimeOffset.UtcNow
                    });

                await dbContext.SaveChangesAsync(stoppingToken);

                // 6. ACK
                await channel.BasicAckAsync(
                    delivery.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);

                logger.LogInformation(
                    "Email {MessageId} processed successfully.",
                    messageId);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Connection closure causes redelivery.
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Unexpected notification processing error.");

                await ScheduleRetryOrStopAsync(
                    channel,
                    delivery,
                    retryPublishFailure,
                    stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        try
        {
            var stop = Task.Delay(
                Timeout.InfiniteTimeSpan,
                stoppingToken);

            if (await Task.WhenAny(stop, retryPublishFailure.Task)
                == retryPublishFailure.Task)
            {
                throw new InvalidOperationException(
                    "Notification retry could not be scheduled.",
                    await retryPublishFailure.Task);
            }

            await stop;
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task<IConnection> ConnectWithRetryAsync(
        ConnectionFactory connectionFactory,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            try
            {
                return await connectionFactory.CreateConnectionAsync(
                    cancellationToken);
            }
            catch (BrokerUnreachableException exception)
            {
                logger.LogWarning(
                    exception,
                    "RabbitMQ is unavailable. Retrying connection in 5 seconds.");

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    cancellationToken);
            }
        }
    }

    private async Task ScheduleRetryOrStopAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        TaskCompletionSource<Exception> retryPublishFailure,
        CancellationToken cancellationToken)
    {
        try
        {
            await ScheduleRetryAsync(
                channel,
                delivery,
                cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Could not schedule notification retry. Stopping the consumer without ACK so RabbitMQ can redeliver the original message.");

            retryPublishFailure.TrySetResult(exception);
        }
    }

    private async Task DeclareRetryQueueAsync(
        IChannel channel,
        string queueName,
        int delayMilliseconds,
        CancellationToken cancellationToken)
    {
        var arguments = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = delayMilliseconds,
            ["x-dead-letter-exchange"] = _options.ExchangeName,
            ["x-dead-letter-routing-key"] =
                MessageRoutingKeys.EmailConfirmationRequested
        };

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: cancellationToken);
    }

    private async Task ScheduleRetryAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        var retryCount = GetRetryCount(
            delivery.BasicProperties.Headers);

        // Initial attempt + 2 retries = 3 attempts
        if (retryCount >= 2)
        {
            logger.LogError(
                "Retry limit reached for {MessageId}. Sending to DLQ.",
                delivery.BasicProperties.MessageId);

            await channel.BasicNackAsync(
                delivery.DeliveryTag,
                multiple: false,
                requeue: false,
                cancellationToken: cancellationToken);

            return;
        }

        var targetQueue = retryCount == 0
            ? _options.Retry10sQueueName
            : _options.Retry30sQueueName;

        var headers = delivery.BasicProperties.Headers is { } original
            ? new Dictionary<string, object?>(original)
            : new Dictionary<string, object?>();

        headers[RetryHeader] = retryCount + 1;

        var properties = new BasicProperties
        {
            MessageId = delivery.BasicProperties.MessageId,
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Headers = headers
        };

        // Publish to retry queue and await publisher confirm.
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: targetQueue,
            mandatory: true,
            basicProperties: properties,
            body: delivery.Body.ToArray(),
            cancellationToken: cancellationToken);

        // Only ACK after RabbitMQ confirms the retry publish.
        await channel.BasicAckAsync(
            delivery.DeliveryTag,
            multiple: false,
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Message {MessageId} scheduled for retry {RetryNumber}.",
            delivery.BasicProperties.MessageId,
            retryCount + 1);
    }

    private static int GetRetryCount(
        IDictionary<string, object?>? headers)
    {
        if (headers is null ||
            !headers.TryGetValue(RetryHeader, out var value))
        {
            return 0;
        }

        return value switch
        {
            int number when number >= 0 => number,
            long number when number >= 0 &&
                             number <= int.MaxValue => (int)number,
            short number when number >= 0 => number,
            byte number => number,
            _ => 2
        };
    }

    private static bool IsTemporaryEmailError(Exception exception)
    {
        return exception switch
        {
            SmtpCommandException smtp =>
                (int)smtp.StatusCode is >= 400 and < 500,

            SmtpProtocolException => true,
            ServiceNotConnectedException => true,
            SocketException => true,
            IOException => true,
            TimeoutException => true,
            OperationCanceledException => true,

            _ => false
        };
    }
}
