using ECommerce.Application.Authorization;
using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;
using ECommerce.Contracts.Messaging;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Constants;

namespace ECommerce.Application.Services;

public sealed class AuthService(
                IIdentityService identityService,
                IJwtTokenGenerator jwtTokenGenerator,
                IUnitOfWork unitOfWork,
                IOutboxWriter outboxWriter,
                ICartRepository cartRepository) : IAuthService
{
    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var emailExists = await identityService.EmailExistsAsync(request.Email);

        if (emailExists)
        {
            return Result<RegisterResponse>.Failure(
                new Error(
                    AuthErrorCodes.EmailAlreadyExists,
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

            return Result<RegisterResponse>.Failure(createResult.Error!);
        }

        var userId = createResult.Value!;

        var cart = new Cart
        {
            UserId = userId
        };

        cartRepository.Add(cart);

        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(userId);

        if (tokenResult.IsFailure)
        {
            await transaction.RollbackAsync();

            return Result<RegisterResponse>.Failure(tokenResult.Error!);
        }

        var message = new EmailConfirmationRequested(userId, request.Email, tokenResult.Value!);

        outboxWriter.Add(message, MessageRoutingKeys.EmailConfirmationRequested);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result<RegisterResponse>.Success(
            new RegisterResponse(userId));
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var userIdResult =
            await identityService.ValidateCredentialsAsync(
                request.Email,
                request.Password);

        if (userIdResult.IsFailure)
        {
            return Result<LoginResponse>.Failure(
                userIdResult.Error!);
        }

        return await GenerateAccessTokenAsync(
            userIdResult.Value!);
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        return await identityService.ConfirmEmailAsync(userId, token);
    }

    public async Task<Result> ResendEmailAsync(string email, CancellationToken cancellationToken)
    {
        var userIdResult = await identityService.GetUnconfirmedUserIdByEmailAsync(email);

        if (userIdResult.IsFailure)
        {
            if (userIdResult.Error!.Code is AuthErrorCodes.UserNotFound or AuthErrorCodes.EmailAlreadyConfirmed)
                return Result.Success();

            return Result.Failure(userIdResult.Error!);
        }

        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(userIdResult.Value!);

        if (tokenResult.IsFailure)
        {
            return Result.Failure(tokenResult.Error!);
        }

        var message = new EmailConfirmationRequested(userIdResult.Value!, email, tokenResult.Value!);

        outboxWriter.Add(message, MessageRoutingKeys.EmailConfirmationRequested);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<LoginResponse>> GenerateAccessTokenAsync(string userId)
    {
        var emailResult = await identityService.GetEmailAsync(userId);

        if (emailResult.IsFailure)
        {
            return Result<LoginResponse>.Failure(emailResult.Error!);
        }

        var rolesResult = await identityService.GetRolesAsync(userId);

        if (rolesResult.IsFailure)
        {
            return Result<LoginResponse>.Failure(rolesResult.Error!);
        }

        var tokenVersionResult = await identityService.GetTokenVersionAsync(userId);

        if (tokenVersionResult.IsFailure)
        {
            return Result<LoginResponse>.Failure(
                tokenVersionResult.Error!);
        }

        var response = jwtTokenGenerator.GenerateToken(
            userId,
            emailResult.Value!,
            rolesResult.Value!,
            tokenVersionResult.Value);

        return Result<LoginResponse>.Success(response);
    }
}
