using AutoMapper;
using ECommerce.Application.Authorization;
using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.DTOs.Sellers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class SellerProfileService(ISellerProfileRepository sellerProfileRepository,
                                  IMapper mapper,
                                  IIdentityService identityService,
                                  IUnitOfWork unitOfWork,
                                  IProductRepository productRepository,
                                  IAuthService authService) : ISellerProfileService
{
    public async Task<Result<CreateSellerProfileResponse>> CreateSellerProfileAsync(
        string userId, 
        CreateSellerProfileDto dto, 
        CancellationToken cancellationToken)
    {
        var existingSellerProfile = await sellerProfileRepository.GetByUserIdAsNoTrackingAsync(
            userId, 
            cancellationToken);

        if (existingSellerProfile is not null)
        {
            return Result<CreateSellerProfileResponse>.Failure(new Error(
                "SellerProfile.AlreadyExists",
                ErrorType.Conflict,
                "Seller profile already exists."));
        }

        var sellerProfile = mapper.Map<SellerProfile>(dto);

        sellerProfile.UserId = userId;
        sellerProfile.CreatedAt = DateTime.UtcNow;

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var roleResult = await identityService.AddToRoleAsync(
            userId,
            AppRoles.Seller);

        if (roleResult.IsFailure)
        {
            return Result<CreateSellerProfileResponse>.Failure(
                roleResult.Error!);
        }

        sellerProfileRepository.Add(sellerProfile);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tokenVersionResult = await identityService.IncrementTokenVersionAsync(userId);

        if (tokenVersionResult.IsFailure)
        {
            return Result<CreateSellerProfileResponse>.Failure(
                tokenVersionResult.Error!);
        }

        var tokenResult = await authService.GenerateAccessTokenAsync(userId);

        if (tokenResult.IsFailure)
        {
            return Result<CreateSellerProfileResponse>.Failure(tokenResult.Error!);
        }

        await transaction.CommitAsync(cancellationToken);

        var sellerProfileDto = mapper.Map<GetSellerProfileDto>(sellerProfile);

        var response = new CreateSellerProfileResponse
        {
            SellerProfile = sellerProfileDto,
            Auth = tokenResult.Value!
        };

        return Result<CreateSellerProfileResponse>.Success(response);
    }

    public async Task<Result<LoginResponse>> DeleteMineSellerProfileAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var sellerProfile = await sellerProfileRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        if (sellerProfile is null)
        {
            return Result<LoginResponse>.Failure(new Error(
                "SellerProfile.NotFound",
                ErrorType.NotFound,
                "Seller profile was not found."));
        }

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(cancellationToken);

        var deleteResult = await DeleteSellerProfileDataAsync(
            sellerProfile,
            cancellationToken);

        if (deleteResult.IsFailure)
        {
            return Result<LoginResponse>.Failure(
                deleteResult.Error!);
        }

        var tokenVersionResult = await identityService.IncrementTokenVersionAsync(userId);

        if (tokenVersionResult.IsFailure)
        {
            return Result<LoginResponse>.Failure(
                tokenVersionResult.Error!);
        }

        var tokenResult = await authService.GenerateAccessTokenAsync(
            userId);

        if (tokenResult.IsFailure)
        {
            return Result<LoginResponse>.Failure(
                tokenResult.Error!);
        }

        await transaction.CommitAsync(cancellationToken);

        return Result<LoginResponse>.Success(
            tokenResult.Value!);
    }

    public async Task<Result> DeleteSellerProfileByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var sellerProfile = await sellerProfileRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (sellerProfile is null)
        {
            return Result.Failure(new Error(
                "SellerProfile.NotFound",
                ErrorType.NotFound,
                "Seller profile was not found."));
        }

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(cancellationToken);

        var deleteResult = await DeleteSellerProfileDataAsync(
            sellerProfile,
            cancellationToken);

        if (deleteResult.IsFailure)
        {
            return deleteResult;
        }

        var tokenVersionResult = await identityService.IncrementTokenVersionAsync(
            sellerProfile.UserId);

        if (tokenVersionResult.IsFailure)
        {
            return tokenVersionResult;
        }

        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result> DeleteSellerProfileDataAsync(
        SellerProfile sellerProfile,
        CancellationToken cancellationToken)
    {
        await productRepository.DeactivateBySellerProfileIdAsync(
            sellerProfile.Id,
            cancellationToken);

        await productRepository.DetachFromSellerProfileAsync(
            sellerProfile.Id,
            cancellationToken);

        var roleResult = await identityService.RemoveFromRoleAsync(
            sellerProfile.UserId,
            AppRoles.Seller);

        if (roleResult.IsFailure)
        {
            return roleResult;
        }

        sellerProfileRepository.Remove(sellerProfile);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<GetSellerProfileDto>> GetMineSellerProfileAsync(
        string userId, 
        CancellationToken cancellationToken)
    {
        var sellerProfile = await sellerProfileRepository.GetByUserIdAsNoTrackingAsync(
            userId,
            cancellationToken);

        if (sellerProfile is null)
        {
            return Result<GetSellerProfileDto>.Failure(new Error(
                "SellerProfile.NotFound",
                ErrorType.NotFound,
                "Seller profile was not found."));
        }

        var sellerProfileDto =
            mapper.Map<GetSellerProfileDto>(sellerProfile);

        return Result<GetSellerProfileDto>.Success(sellerProfileDto);
    }

    public async Task<Result<GetSellerProfileDto>> GetSellerProfileByIdAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var sellerProfile = await sellerProfileRepository.GetByIdAsync(
            id,
            cancellationToken);

        if(sellerProfile is null)
        {
            return Result<GetSellerProfileDto>.Failure(new Error(
                "SellerProfile.NotFound",
                ErrorType.NotFound,
                "Seller profile was not found."));

        }

        var sellerProfileDto = mapper.Map<GetSellerProfileDto>(sellerProfile);

        return Result<GetSellerProfileDto>.Success(sellerProfileDto);
    }

    public async Task<Result<IReadOnlyList<GetSellerProfileListDto>>> GetSellerProfileListAsync(
        CancellationToken cancellationToken)
    {
        var sellerProfiles = await sellerProfileRepository.GetAllAsync(
            cancellationToken);

        var sellerProfileDtos = mapper.Map<IReadOnlyList<GetSellerProfileListDto>>(sellerProfiles);

        return Result<IReadOnlyList<GetSellerProfileListDto>>.Success(
            sellerProfileDtos);
    }

    public async Task<Result> UpdateSellerProfileAsync(
        string userId, 
        UpdateSellerProfileDto dto, 
        CancellationToken cancellationToken)
    {
        var sellerProfile = await sellerProfileRepository
        .GetByUserIdAsync(userId, cancellationToken);

        if (sellerProfile is null)
        {
            return Result.Failure(new Error(
                "SellerProfile.NotFound",
                ErrorType.NotFound,
                "Seller profile was not found."));
        }

        mapper.Map(dto, sellerProfile);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
