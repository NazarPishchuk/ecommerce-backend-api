using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdWithItemsAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<Cart?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<CartItem?> GetItemByProductIdAsync(
        int cartId,
        int productId,
        CancellationToken cancellationToken);

    Task<bool> TryIncreaseItemQuantityAsync(
        int cartId,
        int productId,
        int quantity,
        CancellationToken cancellationToken);

    Task<bool> TryAddItemAsync(
        CartItem cartItem,
        CancellationToken cancellationToken);

    Task<CartItem?> GetItemByIdAsync(
        int cartId,
        int userId,
        CancellationToken cancellationToken);

    Task<bool> TryDecrementItemQuantityAsync(
        int cartId,
        int itemId,
        CancellationToken cancellationToken);
    Task<bool> DeleteItemAsync(
        int cartId,
        int itemId,
        CancellationToken cancellationToken);

    void Add(Cart cart);
}
