using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class CategoryRepository(ECommerceDbContext dbContext) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken)
    {
        return await dbContext.Categories
            .AnyAsync(x => x.NormalizedName == normalizedName, cancellationToken);
    }

    public void Add(Category category)
    {
        dbContext.Categories.Add(category);
    }

    public void Delete(Category category)
    {
        dbContext.Categories.Remove(category);
    }

    public async Task<bool> HasProductsAsync(int categoryId, CancellationToken cancellationToken)
    {
        return await dbContext.Products
                .AnyAsync(product => product.CategoryId == categoryId, cancellationToken);
    }
}

