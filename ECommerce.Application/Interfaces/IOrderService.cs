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

    Task<Result<GetOrderDto>> GetMineByIdAsync(
        int id,
        string userId,
        CancellationToken cancellationToken);

    Task<Result> CancelAsync(
        int id,
        string userId,
        CancellationToken cancellationToken);
}
