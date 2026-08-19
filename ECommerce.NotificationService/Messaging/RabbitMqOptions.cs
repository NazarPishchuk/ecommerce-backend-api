using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.NotificationService.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public required string HostName { get; init; }
    public int Port { get; init; }
    public required string UserName { get; init; }
    public required string Password { get; init; }
    public required string ExchangeName { get; init; }
    public required string QueueName { get; init; }
    public required string DeadLetterExchangeName { get; init; }
    public required string DeadLetterQueueName { get; init; }
}
