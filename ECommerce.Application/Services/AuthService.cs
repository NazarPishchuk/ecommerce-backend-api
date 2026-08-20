using ECommerce.Application.Authorization;
using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Messaging;
using ECommerce.Application.Results;

namespace ECommerce.Application.Services;

public sealed class AuthService(
                IIdentityService identityService,
                IJwtTokenGenerator jwtTokenGenerator,
                IUnitOfWork unitOfWork,
                IOutboxWriter outboxWriter) : IAuthService
{
    public async Task<Result<RegisteredUserResponse>> RegisterAsync(RegisterRequest request,
                                                                    CancellationToken cancellationToken = default)
    {
        var emailExists = await identityService.EmailExistsAsync(request.Email);

        if (emailExists)
        {
            return Result<RegisteredUserResponse>.Failure(
                new Error(
                    "Auth.EmailAlreadyExists",
                    ErrorType.Conflict,
                    "User with this email already exists."));
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var createResult = await identityService.CreateUserAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            AppRoles.Customer);

        if (createResult.IsFailure)
        {
            await transaction.RollbackAsync();

            return Result<RegisteredUserResponse>.Failure(createResult.Error!);
        }

        var userId = createResult.Value!;

        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(userId);

        if (tokenResult.IsFailure)
        {
            await transaction.RollbackAsync();

            return Result<RegisteredUserResponse>.Failure(tokenResult.Error!);
        }

        var message = new EmailConfirmationRequested(userId, request.Email, tokenResult.Value!);

        outboxWriter.Add(message);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result<RegisteredUserResponse>.Success(
            new RegisteredUserResponse(userId));
    }

    public async Task<Result<AccessTokenResponse>> LoginAsync(LoginRequest request)
    {
        var userIdResult = await identityService.ValidateCredentialsAsync(request.Email, request.Password);

        if(userIdResult.IsFailure)
        {
            return Result<AccessTokenResponse>.Failure(userIdResult.Error!);
        }

        var rolesResult = await identityService.GetRolesAsync(userIdResult.Value!);

        if (rolesResult.IsFailure)
        {
            return Result<AccessTokenResponse>.Failure(
                rolesResult.Error!);
        }

        var token = jwtTokenGenerator.GenerateToken(
            userIdResult.Value!,
            request.Email,
            rolesResult.Value!);

        return Result<AccessTokenResponse>.Success(token);
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        return await identityService.ConfirmEmailAsync(userId, token);
    }
}
