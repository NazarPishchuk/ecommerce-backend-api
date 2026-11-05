using System.Data;
using ECommerce.Contracts.Messaging;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Messaging.Payments;

public sealed class PaymentEventHandler(
    ECommerceDbContext dbContext)
{
    public async Task<PaymentEventHandlingResult> HandleAsync(
    PaymentSucceeded message,
    CancellationToken cancellationToken)
    {
        if (message.PaidAtUtc == default ||
            message.PaidAtUtc.Kind != DateTimeKind.Utc)
        {
            return PaymentEventHandlingResult.InvalidPayload;
        }

        var result = await HandleAsync(
            message.MessageId,
            message.PaymentId,
            message.OrderId,
            message.Amount,
            message.Currency,
            message.OccurredAtUtc,
            new DateTimeOffset(message.PaidAtUtc),
            nameof(PaymentSucceeded),
            cancellationToken);

        return result;
    }

    public async Task<PaymentEventHandlingResult> HandleAsync(
    PaymentFailed message,
    CancellationToken cancellationToken)
    {
        var result = await HandleAsync(
            message.MessageId,
            message.PaymentId,
            message.OrderId,
            message.Amount,
            message.Currency,
            message.OccurredAtUtc,
            null,
            nameof(PaymentFailed),
            cancellationToken);

        return result;
    }

    private async Task<PaymentEventHandlingResult> HandleAsync(
        Guid messageId,
        Guid paymentId,
        int orderId,
        decimal amount,
        string currency,
        DateTime occurredAtUtc,
        DateTimeOffset? paidAtUtc,
        string messageType,
        CancellationToken cancellationToken)
    {
        if (messageId == Guid.Empty ||
            paymentId == Guid.Empty ||
            orderId <= 0 ||
            amount <= 0 ||
            currency != "EUR" ||
            occurredAtUtc == default ||
            occurredAtUtc.Kind != DateTimeKind.Utc)
        {
            return PaymentEventHandlingResult.InvalidPayload;
        }

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var existingMessage = await dbContext.InboxMessages
            .AsNoTracking()
            .SingleOrDefaultAsync(
                message => message.Id == messageId,
                cancellationToken);

        if (existingMessage is not null)
        {
            return existingMessage.Type == messageType
                ? PaymentEventHandlingResult.AlreadyProcessed
                : PaymentEventHandlingResult.Conflict;
        }

        var order = await dbContext.Orders
            .SingleOrDefaultAsync(
                order => order.Id == orderId,
                cancellationToken);

        if (order is null)
            return PaymentEventHandlingResult.OrderNotFound;

        if (order.TotalAmount != amount)
            return PaymentEventHandlingResult.Conflict;

        if (order.PaymentId.HasValue != order.PaidAtUtc.HasValue)
            return PaymentEventHandlingResult.Conflict;

        if (paidAtUtc.HasValue)
        {
            if (order.PaymentId.HasValue)
            {
                if (order.PaymentId != paymentId ||
                    order.PaidAtUtc != paidAtUtc)
                {
                    return PaymentEventHandlingResult.Conflict;
                }
            }
            else
            {
                if (order.Status != OrderStatus.Pending)
                    return PaymentEventHandlingResult.Conflict;

                order.PaymentId = paymentId;
                order.PaidAtUtc = paidAtUtc;
            }
        }
        else if (order.PaymentId == paymentId)
        {
            return PaymentEventHandlingResult.Conflict;
        }

        dbContext.InboxMessages.Add(new InboxMessage
        {
            Id = messageId,
            Type = messageType,
            ProcessedAtUtc = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return PaymentEventHandlingResult.Processed;
    }
}
