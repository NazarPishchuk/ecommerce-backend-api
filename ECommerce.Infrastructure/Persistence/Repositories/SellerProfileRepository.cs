using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class SellerProfileRepository(ECommerceDbContext dbContext) : ISellerProfileRepository
{
    public async Task<SellerProfile?> GetByUserIdAsync(
    string userId,
    CancellationToken cancellationToken)
    {
        return await dbContext.SellerProfiles
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public async Task<SellerProfile?> GetByUserIdAsNoTrackingAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.SellerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }

    public void Remove(SellerProfile sellerProfile)
    {
        dbContext.SellerProfiles.Remove(sellerProfile);
    }

    public async Task<SellerProfile?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.SellerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<IReadOnlyList<SellerProfile>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.SellerProfiles
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public void Add(SellerProfile sellerProfile)
    {
        dbContext.SellerProfiles.Add(sellerProfile);
    }
}
