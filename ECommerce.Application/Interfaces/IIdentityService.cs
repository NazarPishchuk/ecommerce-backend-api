using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface IIdentityService
{
    Task<bool> EmailExistsAsync(string email);
    Task<Result<string>> CreateUserAsync(string firstName,
                                string lastName,
                                string email,
                                string password,
                                string role);
                                
    Task<Result<string>> ValidateCredentialsAsync(
                                string email,
                                string password);
    Task<Result<IReadOnlyCollection<string>>> GetRolesAsync(string userId);
    Task<Result<string>> GenerateEmailConfirmationTokenAsync(string userId);
    Task<Result> ConfirmEmailAsync(string userId, string token);

}