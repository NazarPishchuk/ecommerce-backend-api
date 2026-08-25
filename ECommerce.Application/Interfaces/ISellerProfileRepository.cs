using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ISellerProfileRepository
{
    Task<SellerProfile?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<SellerProfile?> GetByUserIdAsNoTrackingAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<SellerProfile?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SellerProfile>> GetAllAsync(
        CancellationToken cancellationToken);

    void Remove(SellerProfile sellerProfile);

    void Add(SellerProfile sellerProfile);
}
