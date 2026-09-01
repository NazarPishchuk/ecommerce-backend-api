using ECommerce.Application.DTOs.Orders.Admin;
using ECommerce.Application.DTOs.Orders.Customer;
using ECommerce.Application.DTOs.Orders.Seller;
using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<Result> CheckoutAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<GetSellerOrderListDto>>> GetMineSellerOrdersAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<Result<GetSellerOrderDetailsDto>> GetMineSellerOrderByIdAsync(
        int id,
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

    Task<Result> ConfirmSellerOrderAsync(
        int id,
        string userId,
        CancellationToken cancellationToken);

    Task<Result> RejectSellerOrderAsync(
        int id,
        string userId,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<GetAdminOrderListDto>>> GetAllForAdminAsync(
        CancellationToken cancellationToken);
}
