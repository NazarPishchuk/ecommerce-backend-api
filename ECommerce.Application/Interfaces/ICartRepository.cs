using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdWithItemsAsync(
        string userId,
        CancellationToken cancellationToken);

    void Add(Cart cart);
}
