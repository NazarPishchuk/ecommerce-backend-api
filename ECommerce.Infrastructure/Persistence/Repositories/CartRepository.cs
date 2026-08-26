using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class CartRepository(
    ECommerceDbContext dbContext) : ICartRepository
{
    public async Task<Cart?> GetByUserIdWithItemsAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Carts
            .Include(x => x.CartItems)
                .ThenInclude(x => x.Product)
                    .ThenInclude(x => x.SellerProfile)
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public async void Add(Cart cart)
    {
        dbContext.Carts.Add(cart);
    }
}
