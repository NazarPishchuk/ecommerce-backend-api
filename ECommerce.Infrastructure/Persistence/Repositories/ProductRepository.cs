using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class ProductRepository(ECommerceDbContext dbContext) : IProductRepository
{
    public async Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.SellerProfile)
            .Where(x =>
                x.IsActive &&
                !x.IsBlocked &&
                x.SellerProfileId.HasValue &&
                dbContext.Users.Any(
                    user =>
                        user.Id == x.SellerProfile!.UserId &&
                        !user.IsSellerBanned))
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(x => x.SellerProfile)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(
                x =>
                    x.Id == id &&
                    x.IsActive &&
                    !x.IsBlocked &&
                    x.SellerProfileId.HasValue &&
                    dbContext.Users.Any(
                        user =>
                            user.Id == x.SellerProfile!.UserId &&
                            !user.IsSellerBanned),
                cancellationToken);
    }

    public async Task<Product?> GetByIdForUpdateAsync(
        int id,
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .FirstOrDefaultAsync(x => x.Id == id,
                                 cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllMineAsync(
        int sellerProfileId, 
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .Where(x => x.SellerProfileId == sellerProfileId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetMineByIdAsync(
        int id, 
        int sellerProfileId, 
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .AsNoTracking()
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.SellerProfileId == sellerProfileId
                                 && x.Id == id, cancellationToken);
    }

    public async Task<Product?> GetMineByIdForUpdateAsync(
        int id, 
        int sellerProfileId, 
        CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .FirstOrDefaultAsync(x => x.SellerProfileId == sellerProfileId
                                 && x.Id == id, cancellationToken);
    }

    public async Task DeactivateBySellerProfileIdAsync(
        int sellerProfileId,
        CancellationToken cancellationToken)
    {
        await dbContext.Products
            .Where(product => product.SellerProfileId == sellerProfileId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    product => product.IsActive,
                    false),
                cancellationToken);
    }

    public async Task DetachFromSellerProfileAsync(
        int sellerProfileId,
        CancellationToken cancellationToken)
    {
        await dbContext.Products
            .Where(product => product.SellerProfileId == sellerProfileId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(
                    product => product.SellerProfileId,
                    (int?)null),
                cancellationToken);
    }

    public void Add(Product product)
    {
        dbContext.Products.Add(product);
    }
}
