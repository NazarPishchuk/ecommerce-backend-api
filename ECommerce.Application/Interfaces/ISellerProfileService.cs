using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.DTOs.Sellers;
using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface ISellerProfileService
{
    Task<Result<GetSellerProfileDto>> GetSellerProfileByIdAsync(
        int id, 
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<GetSellerProfileListDto>>> GetSellerProfileListAsync(
        CancellationToken cancellationToken);

    Task<Result<GetSellerProfileDto>> GetMineSellerProfileAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<Result<CreateSellerProfileResponse>> CreateSellerProfileAsync(
        string userId,
        CreateSellerProfileDto dto,
        CancellationToken cancellationToken);

    Task<Result> UpdateSellerProfileAsync(
            string userId,
            UpdateSellerProfileDto dto,
            CancellationToken cancellationToken);

    Task<Result<LoginResponse>> DeleteMineSellerProfileAsync(
            string userId,
            CancellationToken cancellationToken);

    Task<Result> DeleteSellerProfileByIdAsync(
            int id,
            CancellationToken cancellationToken);

    Task<Result> BanSellerAsync(
        int id,
        CancellationToken cancellationToken);

    Task<Result> UnbanSellerAsync(
        int id,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<GetAdminSellerProfileDto>>> GetAdminSellerProfileListAsync(
        CancellationToken cancellationToken);
}

