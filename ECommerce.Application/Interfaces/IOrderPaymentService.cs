using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface IOrderPaymentService
{
    Task<Result<CreateOrderPaymentResult>> CreatePaymentAsync(
        int orderId,
        string userId,
        CancellationToken cancellationToken);
}