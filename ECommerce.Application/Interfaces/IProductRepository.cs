using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(
        int id, 
        CancellationToken cancellationToken);

    Task<Product?> GetByIdForUpdateAsync(
        int id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetAllMineAsync(
        int sellerProfileId,
        CancellationToken cancellationToken);

    Task<Product?> GetMineByIdAsync(
        int id,
        int sellerProfileId,
        CancellationToken cancellationToken);

    Task<Product?> GetMineByIdForUpdateAsync(
        int id,
        int sellerProfileId,
        CancellationToken cancellationToken);

    void Add(Product product);
}

