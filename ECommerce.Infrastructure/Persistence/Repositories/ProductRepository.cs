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
            .Where(x => x.IsActive && !x.IsBlocked)
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
            .FirstOrDefaultAsync(x => x.Id == id 
                                 && x.IsActive 
                                 && !x.IsBlocked, cancellationToken);
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

    public void Add(Product product)
    {
        dbContext.Products.Add(product);
    }
}
