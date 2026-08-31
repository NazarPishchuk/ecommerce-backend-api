using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;

namespace ECommerce.Application.Services;

public class OrderService(
    ICartRepository cartRepository) : IOrderService
{
    public Task<Result> CheckoutAsync(string userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
 
