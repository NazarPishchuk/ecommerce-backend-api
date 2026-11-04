using System.Text.Json;
using ECommerce.Contracts.Messaging;
using ECommerce.PaymentService.Domain;
using ECommerce.PaymentService.DTOs;
using ECommerce.PaymentService.Persistence;
using ECommerce.PaymentService.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.PaymentService.Services;

public sealed class PaymentWebhookService(
    PaymentDbContext dbContext)
{
    public async Task<PaymentWebhookResult> HandleAsync(
        PaymentWebhookDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.PaymentId == Guid.Empty ||
            string.IsNullOrWhiteSpace(dto.ProviderPaymentId) ||
            dto.ProviderPaymentId.Length > 200)
        {
            return PaymentWebhookResult.InvalidPayload;
        }

        PaymentStatus? targetStatus = dto.Status switch
        {
            "Succeeded" => PaymentStatus.Succeeded,
            "Failed" => PaymentStatus.Failed,
            _ => null
        };

        if (targetStatus is null)
            return PaymentWebhookResult.InvalidPayload;

        if (targetStatus == PaymentStatus.Succeeded &&
            (dto.PaidAt is null ||
             dto.PaidAt.Value.Kind != DateTimeKind.Utc))
        {
            return PaymentWebhookResult.InvalidPayload;
        }

        if (targetStatus == PaymentStatus.Failed &&
            dto.PaidAt is not null)
        {
            return PaymentWebhookResult.InvalidPayload;
        }

        var status = targetStatus.Value;

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        var affectedRows = await dbContext.Payments
            .Where(payment =>
                payment.Id == dto.PaymentId &&
                payment.ProviderPaymentId == dto.ProviderPaymentId &&
                payment.Status == PaymentStatus.Pending)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(payment => payment.Status, status)
                    .SetProperty(payment => payment.PaidAt, dto.PaidAt),
                cancellationToken);

        if (affectedRows == 1)
        {
            var payment = await dbContext.Payments
                .AsNoTracking()
                .SingleAsync(
                    payment => payment.Id == dto.PaymentId,
                    cancellationToken);

            var messageId = Guid.NewGuid();
            var occurredAtUtc = DateTime.UtcNow;

            string type;
            string routingKey;
            string payload;

            if (status == PaymentStatus.Succeeded)
            {
                var message = new PaymentSucceeded(
                    messageId,
                    payment.Id,
                    payment.OrderId,
                    payment.Amount,
                    payment.Currency,
                    dto.PaidAt!.Value,
                    occurredAtUtc);

                type = nameof(PaymentSucceeded);
                routingKey = MessageRoutingKeys.PaymentSucceeded;
                payload = JsonSerializer.Serialize(message);
            }
            else
            {
                var message = new PaymentFailed(
                    messageId,
                    payment.Id,
                    payment.OrderId,
                    payment.Amount,
                    payment.Currency,
                    occurredAtUtc);

                type = nameof(PaymentFailed);
                routingKey = MessageRoutingKeys.PaymentFailed;
                payload = JsonSerializer.Serialize(message);
            }

            dbContext.OutboxMessages.Add(new OutboxMessage
            {
                Id = messageId,
                Type = type,
                RoutingKey = routingKey,
                Payload = payload,
                OccurredAtUtc = new DateTimeOffset(occurredAtUtc)
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return PaymentWebhookResult.Processed;
        }

        await transaction.RollbackAsync(cancellationToken);

        var existingPayment = await dbContext.Payments
            .AsNoTracking()
            .SingleOrDefaultAsync(
                payment => payment.Id == dto.PaymentId,
                cancellationToken);

        if (existingPayment is null)
            return PaymentWebhookResult.NotFound;

        if (existingPayment.ProviderPaymentId != dto.ProviderPaymentId)
            return PaymentWebhookResult.Conflict;

        if (existingPayment.Status == status &&
            existingPayment.PaidAt == dto.PaidAt)
        {
            return PaymentWebhookResult.AlreadyProcessed;
        }

        return PaymentWebhookResult.Conflict;
    }
}
