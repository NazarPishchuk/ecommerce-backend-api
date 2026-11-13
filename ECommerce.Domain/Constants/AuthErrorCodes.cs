namespace ECommerce.Domain.Constants;

public static class AuthErrorCodes
{
    public const string EmailAlreadyExists = "Auth.EmailAlreadyExists";
    public const string InvalidCredentials = "Auth.InvalidCredentials";
    public const string AccountLocked = "Auth.AccountLocked";
    public const string SignInNotAllowed = "Auth.SignInNotAllowed";
    public const string UserNotFound = "Auth.UserNotFound";
    public const string EmailAlreadyConfirmed = "Auth.EmailAlreadyConfirmed";
}
