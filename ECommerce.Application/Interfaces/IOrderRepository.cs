using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IOrderRepository
{
    void Add(Order order);

    Task<IReadOnlyList<Order>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<Order?> GetByIdAndUserIdAsync(
        int id,
        string userId,
        CancellationToken cancellationToken);

    Task<Order?> GetByIdAndUserIdForUpdateAsync(
        int id,
        string userId,
        CancellationToken cancellationToken);

    Task<bool> TryCancelPendingSellerOrdersAsync(
        int orderId,
        string userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SellerOrder>> GetSellerOrdersByUserIdAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<SellerOrder?> GetSellerOrderByIdAndUserIdAsync(
        int id,
        string userId,
        CancellationToken cancellationToken);

    Task<bool> TryConfirmSellerOrderAsync(
        int id,
        string userId,
        CancellationToken cancellationToken);

    Task MarkOrderAsProcessingAsync(
        int orderId,
        CancellationToken cancellationToken);
}
