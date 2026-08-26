using ECommerce.Application.DTOs.Sellers;
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

    public async Task<IReadOnlyList<GetAdminSellerProfileDto>> GetAllForAdminAsync(
        CancellationToken cancellationToken)
    {
        return await (
            from sellerProfile in dbContext.SellerProfiles.AsNoTracking()
            join user in dbContext.Users
                on sellerProfile.UserId equals user.Id
            select new GetAdminSellerProfileDto
            {
                Id = sellerProfile.Id,
                UserId = sellerProfile.UserId,
                StoreName = sellerProfile.StoreName,
                Description = sellerProfile.Description,
                CreatedAt = sellerProfile.CreatedAt,
                IsBanned = user.IsSellerBanned
            })
            .ToListAsync(cancellationToken);
    }
    public void Add(SellerProfile sellerProfile)
    {
        dbContext.SellerProfiles.Add(sellerProfile);
    }

    public async Task<SellerProfile?> GetPublicByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await dbContext.SellerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(
                sellerProfile =>
                    sellerProfile.Id == id &&
                    dbContext.Users.Any(
                        user =>
                            user.Id == sellerProfile.UserId &&
                            !user.IsSellerBanned),
                cancellationToken);
    }

    public async Task<IReadOnlyList<SellerProfile>> GetAllPublicAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.SellerProfiles
            .AsNoTracking()
            .Where(
                sellerProfile =>
                    dbContext.Users.Any(
                        user =>
                            user.Id == sellerProfile.UserId &&
                            !user.IsSellerBanned))
            .ToListAsync(cancellationToken);
    }
}
