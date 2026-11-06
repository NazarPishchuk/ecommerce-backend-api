using ECommerce.PaymentService.Domain;
using ECommerce.PaymentService.DTOs;
using ECommerce.PaymentService.Persistence;
using ECommerce.PaymentService.Providers;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.PaymentService.Services;

public class PaymentService(
        PaymentDbContext dbContext,
        IPaymentProvider paymentProvider) : IPaymentService
{

    public async Task<PaymentCreationResult> CreatePaymentAsync(
    int orderId,
    decimal amount,
    string currency,
    CancellationToken cancellationToken = default)
    {
        var normalizedCurrency = currency.ToUpperInvariant();

        var alreadyPaid = await dbContext.Payments
            .AnyAsync(
                payment =>
                    payment.OrderId == orderId &&
                    payment.Status == PaymentStatus.Succeeded,
                cancellationToken);

        if (alreadyPaid)
        {
            return new PaymentCreationResult(
                PaymentCreationStatus.AlreadyPaid,
                null);
        }

        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(
                payment =>
                    payment.OrderId == orderId &&
                    payment.Status == PaymentStatus.Pending,
                cancellationToken);

        var isNew = payment is null;

        if (payment is not null &&
            (payment.Amount != amount ||
             !string.Equals(
                 payment.Currency,
                 normalizedCurrency,
                 StringComparison.OrdinalIgnoreCase)))
        {
            return new PaymentCreationResult(
                PaymentCreationStatus.Conflict,
                null);
        }

        if (payment is null)
        {
            payment = new Payment
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                Amount = amount,
                Currency = normalizedCurrency,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Payments.Add(payment);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(payment.PaymentUrl))
        {
            var providerResult = await paymentProvider.CreatePaymentAsync(
                payment.Id,
                payment.Amount,
                payment.Currency,
                cancellationToken);

            payment.ProviderPaymentId = providerResult.ProviderPaymentId;
            payment.PaymentUrl = providerResult.PaymentUrl;

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new PaymentCreationResult(
            isNew
                ? PaymentCreationStatus.Created
                : PaymentCreationStatus.Reused,
            new CreatePaymentResultDto(
                payment.Id,
                payment.Status,
                payment.PaymentUrl!));
    }

    public async Task<GetPaymentDto?> GetPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.Id == paymentId)
            .Select(p => new GetPaymentDto(
                p.Id,
                p.OrderId,
                p.Amount,
                p.Currency,
                p.Status,
                p.CreatedAt,
                p.PaidAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
