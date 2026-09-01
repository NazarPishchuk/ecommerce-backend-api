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
}
