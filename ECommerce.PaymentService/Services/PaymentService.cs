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

    public async Task<CreatePaymentResultDto> CreatePaymentAsync(
        int orderId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default)
    {
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Amount = amount,
            Currency = currency.ToUpperInvariant(),
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        var providerResult = await paymentProvider.CreatePaymentAsync(
            payment.Id,
            payment.Amount,
            payment.Currency,
            cancellationToken);

        payment.ProviderPaymentId = providerResult.ProviderPaymentId;

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreatePaymentResultDto(
            payment.Id,
            payment.Status,
            providerResult.PaymentUrl);
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
