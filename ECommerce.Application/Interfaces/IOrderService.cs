using ECommerce.Application.Results;
using ECommerce.Application.DTOs.Orders;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<Result> CheckoutAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<GetOrderListDto>>> GetMineAsync(
        string userId,
        CancellationToken cancellationToken);
}
