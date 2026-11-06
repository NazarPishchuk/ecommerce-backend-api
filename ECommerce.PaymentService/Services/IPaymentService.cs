using ECommerce.PaymentService.DTOs;

namespace ECommerce.PaymentService.Services;

public interface IPaymentService
{
    Task<PaymentCreationResult> CreatePaymentAsync(
        int orderId,
        decimal amount,
        string currency,
        CancellationToken cancellationToken = default);

    Task<GetPaymentDto?> GetPaymentAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);
}
