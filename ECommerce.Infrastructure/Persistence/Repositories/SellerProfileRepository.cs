using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class SellerProfileRepository(ECommerceDbContext dbContext) : ISellerProfileRepository
{
    public async Task<SellerProfile?> GetSellerByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.SellerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
            x => x.UserId == userId,
            cancellationToken);
    }
}
