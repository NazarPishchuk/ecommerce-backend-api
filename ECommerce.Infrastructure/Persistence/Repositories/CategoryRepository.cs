using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Infrastructure.Persistence.Repositories;

public class CategoryRepository(ECommerceDbContext dbContext) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        return await dbContext.Categories
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await dbContext.Categories
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsByNormalizedNameAsync(string normalizedName)
    {
        return await dbContext.Categories
            .AnyAsync(x => x.NormalizedName == normalizedName);
    }

    public void Add(Category category)
    {
        dbContext.Categories.Add(category);
    }

    public void Delete(Category category)
    {
        dbContext.Categories.Remove(category);
    }

    public async Task<bool> HasProductsAsync(int categoryId)
    {
        return await dbContext.Products
                .AnyAsync(product => product.CategoryId == categoryId);
    }
}

