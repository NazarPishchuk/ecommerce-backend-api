using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface ISellerProfileRepository
{
    Task<SellerProfile?> GetSellerByUserIdAsync(
        string userId,
        CancellationToken cancellationToken);
}
