namespace ECommerce.Domain.Constants;

public static class IdentityErrorCodes
{
    public const string UserCreationFailed = "Identity.UserCreationFailed";
    public const string RoleAssignmentFailed = "Identity.RoleAssignmentFailed";
    public const string UserNotFound = "Identity.UserNotFound";
    public const string EmailConfirmationFailed = "Identity.EmailConfirmationFailed";
    public const string RoleRemovalFailed = "Identity.RoleRemovalFailed";
    public const string TokenVersionUpdateFailed = "Identity.TokenVersionUpdateFailed";
    public const string SellerBanFailed = "Identity.SellerBanFailed";
    public const string SellerUnbanFailed = "Identity.SellerUnbanFailed";
}
