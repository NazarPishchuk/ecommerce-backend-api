using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<Result> CheckoutAsync(
        string userId,
        CancellationToken cancellationToken);
}
