using System.Text.Json;
using ECommerce.Contracts.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommerce.Infrastructure.Messaging.Payments;

public sealed class PaymentEventsConsumer(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<PaymentEventsConsumer> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            ClientProvidedName = "ecommerce-api-payment-consumer",
            AutomaticRecoveryEnabled = false,
            ConsumerDispatchConcurrency = 1
        };

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ConsumeAsync(factory, stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    logger.LogError(
                        exception,
                        "Payment consumer connection failed.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task ConsumeAsync(
        ConnectionFactory factory,
        CancellationToken stoppingToken)
    {
        await using var connection =
            await factory.CreateConnectionAsync(stoppingToken);

        await using var channel =
            await connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

        var sessionEnded = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        channel.ChannelShutdownAsync += (_, _) =>
        {
            sessionEnded.TrySetResult(true);
            return Task.CompletedTask;
        };

        await PaymentQueueTopology.DeclareAsync(
            channel,
            _options.ExchangeName,
            stoppingToken);

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.UnregisteredAsync += (_, _) =>
        {
            sessionEnded.TrySetResult(true);
            return Task.CompletedTask;
        };

        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                var processed = await ProcessWithRetryAsync(
                    delivery,
                    stoppingToken);

                if (processed)
                {
                    await channel.BasicAckAsync(
                        deliveryTag: delivery.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken);
                }
                else
                {
                    await channel.BasicNackAsync(
                        deliveryTag: delivery.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                sessionEnded.TrySetResult(true);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Payment consumer could not finish delivery {DeliveryTag}.",
                    delivery.DeliveryTag);

                sessionEnded.TrySetResult(true);
            }
        };

        await channel.BasicConsumeAsync(
            queue: PaymentQueueTopology.QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        logger.LogInformation(
            "Payment consumer is listening on {QueueName}.",
            PaymentQueueTopology.QueueName);

        await sessionEnded.Task.WaitAsync(stoppingToken);
    }

    private async Task<bool> ProcessWithRetryAsync(
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        var body = delivery.Body.ToArray();

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                await using var scope =
                    scopeFactory.CreateAsyncScope();

                var handler = scope.ServiceProvider
                    .GetRequiredService<PaymentEventHandler>();

                var result = await DispatchAsync(
                    handler,
                    delivery,
                    body,
                    cancellationToken);

                if (result is PaymentEventHandlingResult.Processed
                    or PaymentEventHandlingResult.AlreadyProcessed)
                {
                    logger.LogInformation(
                        "Payment event {MessageId} handled with result {Result}.",
                        delivery.BasicProperties.MessageId,
                        result);

                    return true;
                }

                logger.LogWarning(
                    "Payment event {MessageId} rejected with result {Result}.",
                    delivery.BasicProperties.MessageId,
                    result);

                return false;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (JsonException exception)
            {
                logger.LogWarning(
                    exception,
                    "Payment event {MessageId} contains invalid JSON.",
                    delivery.BasicProperties.MessageId);

                return false;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Payment event {MessageId} failed on attempt {Attempt}.",
                    delivery.BasicProperties.MessageId,
                    attempt);

                if (attempt == 3)
                    return false;
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                cancellationToken);
        }

        return false;
    }

    private static async Task<PaymentEventHandlingResult> DispatchAsync(
        PaymentEventHandler handler,
        BasicDeliverEventArgs delivery,
        byte[] body,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(
                delivery.BasicProperties.MessageId,
                out var messageId))
        {
            return PaymentEventHandlingResult.InvalidPayload;
        }

        switch (delivery.RoutingKey)
        {
            case MessageRoutingKeys.PaymentSucceeded:
                {
                    var message =
                        JsonSerializer.Deserialize<PaymentSucceeded>(body);

                    if (message is null ||
                        message.MessageId != messageId ||
                        delivery.BasicProperties.Type != nameof(PaymentSucceeded))
                    {
                        return PaymentEventHandlingResult.InvalidPayload;
                    }

                    var result = await handler.HandleAsync(
                        message,
                        cancellationToken);

                    return result;
                }

            case MessageRoutingKeys.PaymentFailed:
                {
                    var message =
                        JsonSerializer.Deserialize<PaymentFailed>(body);

                    if (message is null ||
                        message.MessageId != messageId ||
                        delivery.BasicProperties.Type != nameof(PaymentFailed))
                    {
                        return PaymentEventHandlingResult.InvalidPayload;
                    }

                    var result = await handler.HandleAsync(
                        message,
                        cancellationToken);

                    return result;
                }

            default:
                return PaymentEventHandlingResult.InvalidPayload;
        }
    }
}
