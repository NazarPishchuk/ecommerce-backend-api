namespace ECommerce.Contracts.Messaging;

public sealed record PaymentFailed(
    Guid MessageId,
    Guid PaymentId,
    int OrderId,
    decimal Amount,
    string Currency,
    DateTime OccurredAtUtc);
