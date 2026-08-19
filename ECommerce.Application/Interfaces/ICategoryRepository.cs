using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken);
    Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);
    void Add(Category category);
    void Delete(Category category);
    Task<bool> HasProductsAsync(int categoryId, CancellationToken cancellationToken);
}
