using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;
using ECommerce.Domain.Constants;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Identity;

public sealed class IdentityService(
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager) : IIdentityService
{
    public async Task<bool> EmailExistsAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        return user is not null;
    }

    public async Task<Result<string>> CreateUserAsync(
                string firstName,
                string lastName,
                string email,
                string password,
                string role)
    {

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var createResult = await userManager.CreateAsync(user, password);

        if (!createResult.Succeeded)
        {
            var errorMessage = string.Join(
                "; ",
                createResult.Errors.Select(error => error.Description));

            return Result<string>.Failure(
                new Error(
                    IdentityErrorCodes.UserCreationFailed,
                    ErrorType.Validation,
                    errorMessage));
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {

            var errorMessage = string.Join(", ",
                    roleResult.Errors.Select(error => error.Description));

            return Result<string>.Failure(
                new Error(
                    IdentityErrorCodes.RoleAssignmentFailed,
                    ErrorType.Validation,
                    errorMessage));
        }

        return Result<string>.Success(user.Id);
    }

    public async Task<Result<IReadOnlyCollection<string>>> GetRolesAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if(user is null)
        {
            return Result<IReadOnlyCollection<string>>.Failure(
                new Error(
                    IdentityErrorCodes.UserNotFound,
                    ErrorType.NotFound,
                    "User was not found."));
        }

        var roles = await userManager.GetRolesAsync(user);

        return Result<IReadOnlyCollection<string>>.Success(roles.ToArray());
    }

    public async Task<Result<string>> ValidateCredentialsAsync(string email, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        
        if(user is null)
        {
            return Result<string>.Failure(
                new Error(
                    AuthErrorCodes.InvalidCredentials,
                    ErrorType.Unauthorized,
                    "Invalid email or password."));
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(
        user,
        password,
        lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
        {
            return Result<string>.Failure(
                new Error(
                    AuthErrorCodes.AccountLocked,
                    ErrorType.Forbidden,
                    "Account is temporarily locked."));
        }

        if (signInResult.IsNotAllowed)
        {
            return Result<string>.Failure(
                new Error(
                    AuthErrorCodes.SignInNotAllowed,
                    ErrorType.Forbidden,
                    "Sign-in is not allowed for this account."));
        }

        if (!signInResult.Succeeded)
        {
            return Result<string>.Failure(
                new Error(
                    AuthErrorCodes.InvalidCredentials,
                    ErrorType.Unauthorized,
                    "Invalid email or password."));
        }

        return Result<string>.Success(user.Id);
    }

    public async Task<Result<string>> GenerateEmailConfirmationTokenAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if(user is null)
        {
            return Result<string>.Failure(
            new Error(
                IdentityErrorCodes.UserNotFound,
                ErrorType.NotFound,
                "User was not found."));
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        return Result<string>.Success(token);
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);

        if(user is null)
        {
            return Result.Failure(
                new Error(
                    IdentityErrorCodes.UserNotFound,
                    ErrorType.NotFound,
                    "User was not found."));
        }

        var confirmResult = await userManager.ConfirmEmailAsync(user, token);

        if (!confirmResult.Succeeded)
        {
            var errorMessage = string.Join(
                "; ",
                confirmResult.Errors.Select(error => error.Description));

            return Result.Failure(
                new Error(
                    IdentityErrorCodes.EmailConfirmationFailed,
                    ErrorType.Validation,
                    errorMessage));
        }

        return Result.Success();
    }
    public async Task<Result<string>> GetUnconfirmedUserIdByEmailAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);

        if(user is null)
        {
            return Result<string>.Failure(
                new Error(
                    AuthErrorCodes.UserNotFound,
                    ErrorType.NotFound,
                    "User with this email was not found."));
        }

        if (user.EmailConfirmed)
        {
            return Result<string>.Failure(
                new Error(
                    AuthErrorCodes.EmailAlreadyConfirmed,
                    ErrorType.Conflict,
                    "Email is already confirmed."));
        }

        return Result<string>.Success(user.Id);
    }

    public async Task<Result> AddToRoleAsync(string userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure(
                new Error(
                    IdentityErrorCodes.UserNotFound,
                    ErrorType.NotFound,
                    "User was not found."));
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            var errorMessage = string.Join(
                "; ",
                roleResult.Errors.Select(error => error.Description));

            return Result.Failure(
                new Error(
                    IdentityErrorCodes.RoleAssignmentFailed,
                    ErrorType.Validation,
                    errorMessage));
        }

        return Result.Success();
    }

    public async Task<Result> RemoveFromRoleAsync(
        string userId,
        string role)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure(new Error(
                IdentityErrorCodes.UserNotFound,
                ErrorType.NotFound,
                "User was not found."));
        }

        var roleResult = await userManager.RemoveFromRoleAsync(
            user,
            role);

        if (!roleResult.Succeeded)
        {
            var errorMessage = string.Join(
                "; ",
                roleResult.Errors.Select(error => error.Description));

            return Result.Failure(new Error(
                IdentityErrorCodes.RoleRemovalFailed,
                ErrorType.Validation,
                errorMessage));
        }

        return Result.Success();
    }

    public async Task<Result<string>> GetEmailAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if(user is null)
        {
            return Result<string>.Failure(new Error(
            IdentityErrorCodes.UserNotFound,
            ErrorType.NotFound,
            "User was not found."));
        }

        return Result<string>.Success(user.Email!);
    }

    public async Task<Result<int>> GetTokenVersionAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result<int>.Failure(new Error(
                IdentityErrorCodes.UserNotFound,
                ErrorType.NotFound,
                "User was not found."));
        }

        return Result<int>.Success(user.TokenVersion);
    }

    public async Task<Result> IncrementTokenVersionAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure(new Error(
                IdentityErrorCodes.UserNotFound,
                ErrorType.NotFound,
                "User was not found."));
        }

        user.TokenVersion++;

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                updateResult.Errors.Select(error => error.Description));

            return Result.Failure(new Error(
                IdentityErrorCodes.TokenVersionUpdateFailed,
                ErrorType.Failure,
                errors));
        }

        return Result.Success();
    }

    public async Task<Result<bool>> IsSellerBannedAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result<bool>.Failure(new Error(
                IdentityErrorCodes.UserNotFound,
                ErrorType.NotFound,
                "User was not found."));
        }

        return Result<bool>.Success(user.IsSellerBanned);
    }

    public async Task<Result> BanSellerAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure(new Error(
                IdentityErrorCodes.UserNotFound,
                ErrorType.NotFound,
                "User was not found."));
        }

        user.IsSellerBanned = true;

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                updateResult.Errors.Select(error => error.Description));

            return Result.Failure(new Error(
                IdentityErrorCodes.SellerBanFailed,
                ErrorType.Failure,
                errors));
        }

        return Result.Success();
    }

    public async Task<Result> UnbanSellerAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result.Failure(new Error(
                IdentityErrorCodes.UserNotFound,
                ErrorType.NotFound,
                "User was not found."));
        }

        user.IsSellerBanned = false;

        var updateResult = await userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                updateResult.Errors.Select(error => error.Description));

            return Result.Failure(new Error(
                IdentityErrorCodes.SellerUnbanFailed,
                ErrorType.Failure,
                errors));
        }

        return Result.Success();
    }
}
