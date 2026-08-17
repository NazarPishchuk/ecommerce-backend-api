using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace ECommerce.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public required string HostName { get; init; }
    public int Port { get; set; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required string ExchangeName { get; init; }
}
