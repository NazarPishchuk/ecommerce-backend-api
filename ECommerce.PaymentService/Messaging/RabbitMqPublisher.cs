using System.Text;
using ECommerce.PaymentService.Persistence.Outbox;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ECommerce.PaymentService.Messaging;

public sealed class RabbitMqPublisher(
    IOptions<RabbitMqOptions> options) : IAsyncDisposable
{
    private readonly RabbitMqOptions _options = options.Value;

    private readonly SemaphoreSlim _gate = new(1, 1);

    private readonly ConnectionFactory _connectionFactory = new()
    {
        HostName = options.Value.HostName,
        Port = options.Value.Port,
        UserName = options.Value.UserName,
        Password = options.Value.Password,
        ClientProvidedName = "ecommerce-payment-publisher",
        AutomaticRecoveryEnabled = false
    };

    private IConnection? _connection;
    private IChannel? _channel;

    public async Task PublishAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            using var timeout =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            timeout.CancelAfter(TimeSpan.FromSeconds(15));

            await EnsureConnectedAsync(timeout.Token);

            var properties = new BasicProperties
            {
                MessageId = message.Id.ToString(),
                Type = message.Type,
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent
            };

            var body = Encoding.UTF8.GetBytes(message.Payload);

            await _channel!.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: message.RoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: timeout.Token);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task EnsureConnectedAsync(
        CancellationToken cancellationToken)
    {
        if (_connection is null || !_connection.IsOpen)
        {
            if (_channel is not null)
            {
                await _channel.DisposeAsync();
                _channel = null;
            }

            if (_connection is not null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }

            _connection = await _connectionFactory.CreateConnectionAsync(
                cancellationToken);
        }

        if (_channel is not null && _channel.IsOpen)
            return;

        if (_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }

        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        var channel = await _connection.CreateChannelAsync(
            channelOptions,
            cancellationToken);

        try
        {
            await channel.ExchangeDeclareAsync(
                exchange: _options.ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken);

            _channel = channel;
        }
        catch
        {
            await channel.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_channel is not null)
                await _channel.DisposeAsync();
        }
        finally
        {
            try
            {
                if (_connection is not null)
                    await _connection.DisposeAsync();
            }
            finally
            {
                _gate.Dispose();
            }
        }
    }
}
