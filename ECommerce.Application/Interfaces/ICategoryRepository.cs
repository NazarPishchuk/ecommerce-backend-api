using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> ExistsByNormalizedNameAsync(string normalizedName);
    void Add(Category category);
    void Delete(Category category);
    Task<bool> HasProductsAsync(int categoryId);
}
