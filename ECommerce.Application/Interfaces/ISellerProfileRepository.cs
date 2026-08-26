using ECommerce.Application.DTOs.Sellers;
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

    Task<IReadOnlyList<GetAdminSellerProfileDto>> GetAllForAdminAsync(
        CancellationToken cancellationToken);

    void Remove(SellerProfile sellerProfile);

    void Add(SellerProfile sellerProfile);

    Task<SellerProfile?> GetPublicByIdAsync(
        int id,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<SellerProfile>> GetAllPublicAsync(
        CancellationToken cancellationToken);

    Task<IReadOnlySet<int>> GetBannedSellerProfileIdsAsync(
        IEnumerable<int> sellerProfileIds,
        CancellationToken cancellationToken);
}
