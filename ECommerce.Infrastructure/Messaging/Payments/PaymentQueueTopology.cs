using ECommerce.Contracts.Messaging;
using RabbitMQ.Client;

namespace ECommerce.Infrastructure.Messaging.Payments;

public static class PaymentQueueTopology
{
    public const string QueueName = "orders.payment-results";

    public const string DeadLetterExchangeName =
        "orders.payment-results.dead-letter";

    public const string DeadLetterQueueName =
        "orders.payment-results.dlq";

    public const string DeadLetterRoutingKey =
        "orders.payment-results.rejected";

    public static async Task DeclareAsync(
        IChannel channel,
        string exchangeName,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum"
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: DeadLetterQueueName,
            exchange: DeadLetterExchangeName,
            routingKey: DeadLetterRoutingKey,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-dead-letter-exchange"] = DeadLetterExchangeName,
                ["x-dead-letter-routing-key"] = DeadLetterRoutingKey,
                ["x-dead-letter-strategy"] = "at-least-once",
                ["x-overflow"] = "reject-publish"
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: exchangeName,
            routingKey: MessageRoutingKeys.PaymentSucceeded,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: exchangeName,
            routingKey: MessageRoutingKeys.PaymentFailed,
            cancellationToken: cancellationToken);
    }
}