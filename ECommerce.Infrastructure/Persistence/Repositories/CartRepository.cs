using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class CartRepository(
    ECommerceDbContext dbContext) : ICartRepository
{
    public async Task<Cart?> GetByUserIdWithItemsAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Carts
            .AsNoTracking()
            .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public async Task<Cart?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Carts
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public async Task<CartItem?> GetItemByProductIdAsync(
        int cartId,
        int productId,
        CancellationToken cancellationToken)
    {
        return await dbContext.CartItems
            .FirstOrDefaultAsync(
                x => x.CartId == cartId &&
                     x.ProductId == productId,
                cancellationToken);
    }

    public async Task<bool> TryIncreaseItemQuantityAsync(
        int cartId,
        int productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        var affectedRows = await dbContext.CartItems
            .Where(x =>
                x.CartId == cartId &&
                x.ProductId == productId &&
                x.Quantity + quantity <= x.Product.Quantity)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        x => x.Quantity,
                        x => x.Quantity + quantity),
                cancellationToken);

        return affectedRows == 1;
    }

    public async Task<bool> TryAddItemAsync(
       CartItem cartItem,
       CancellationToken cancellationToken)
    {
        dbContext.CartItems.Add(cartItem);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (DbUpdateException exception)
            when (IsUniqueConstraintViolation(exception))
        {
            dbContext.Entry(cartItem).State = EntityState.Detached;

            return false;
        }
    }

    public void Add(Cart cart)
    {
        dbContext.Carts.Add(cart);
    }

    private static bool IsUniqueConstraintViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException &&
               (sqlException.Number == 2601 ||
                sqlException.Number == 2627);
    }

}
