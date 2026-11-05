namespace ECommerce.PaymentService.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public required string HostName { get; init; }

    public int Port { get; init; } = 5672;

    public required string UserName { get; init; }

    public required string Password { get; init; }

    public required string ExchangeName { get; init; }
}
