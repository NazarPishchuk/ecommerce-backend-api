using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;

namespace ECommerce.Infrastructure.Messaging;

public sealed class RabbitMqPublisher(IOptions<RabbitMqOptions> options)
{
    private readonly RabbitMqOptions _options = options.Value;

    private readonly ConnectionFactory _connectionFactory = new()
    {
        HostName = options.Value.HostName,
        Port = options.Value.Port,
        UserName = options.Value.UserName,
        Password = options.Value.Password,
        ClientProvidedName = "ecommerce-api-publisher"
    };

    private IConnection? _connection;
    private IChannel? _channel;


    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_connection is null || !_connection.IsOpen)
        {
            _connection = await _connectionFactory
                .CreateConnectionAsync(cancellationToken);
        }

        if (_channel is null || !_channel.IsOpen)
        {
            var channelOptions = new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true);

            _channel = await _connection.CreateChannelAsync(
                channelOptions,
                cancellationToken);
        }
    }

    public async Task PublishAsync(string routingKey, string payload, CancellationToken cancellationToken)
    {
        await EnsureConnectedAsync(cancellationToken);

        await _channel!.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(payload);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent
        };

        await _channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }
}