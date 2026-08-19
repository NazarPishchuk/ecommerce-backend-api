using ECommerce.Contracts.Messaging;
using ECommerce.NotificationService.Email;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;

namespace ECommerce.NotificationService.Messaging;

internal sealed class EmailConfirmationRequestedConsumer(
    IOptions<RabbitMqOptions> options,
    ILogger<EmailConfirmationRequestedConsumer> logger,
    IEmailSender emailSender)
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
            AutomaticRecoveryEnabled = true
        };

        await using var connection =
            await connectionFactory.CreateConnectionAsync(stoppingToken);

        await using var channel =
            await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Normal exchange
        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Exchange for failed messages
        await channel.ExchangeDeclareAsync(
            exchange: _options.DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        // Queue where failed messages will stay
        await channel.QueueDeclareAsync(
            queue: _options.DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken);

        // Connect DLQ to dead-letter exchange
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

        // Main queue
        await channel.QueueDeclareAsync(
            queue: _options.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: stoppingToken);

        // Normal exchange -> main queue
        await channel.QueueBindAsync(
            queue: _options.QueueName,
            exchange: _options.ExchangeName,
            routingKey:
                MessageRoutingKeys.EmailConfirmationRequested,
            cancellationToken: stoppingToken);

        var consumer =
            new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var body = eventArgs.Body.ToArray();

                var message =
                    JsonSerializer
                        .Deserialize<EmailConfirmationRequested>(
                            body);

                if (message is null)
                {
                    throw new InvalidOperationException(
                        "RabbitMQ message could not be deserialized.");
                }

                logger.LogInformation(
                    "Email confirmation requested for {Email}",
                    message.Email);

                await emailSender.SendConfirmationEmailAsync(
                    message.Email,
                    message.UserId,
                    message.ConfirmationToken,
                    stoppingToken);

                logger.LogInformation(
                    "Confirmation email sent to {Email}",
                    message.Email);

                await channel.BasicAckAsync(
                    eventArgs.DeliveryTag,
                    multiple: false);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to process email confirmation message.");

                await channel.BasicNackAsync(
                    eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: false);
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