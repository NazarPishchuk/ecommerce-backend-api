namespace ECommerce.Contracts.Messaging;

public sealed record PaymentSucceeded(
    Guid MessageId,
    Guid PaymentId,
    int OrderId,
    decimal Amount,
    string Currency,
    DateTime PaidAtUtc,
    DateTime OccurredAtUtc);
