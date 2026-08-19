namespace ECommerce.Contracts.Messaging;

public sealed record EmailConfirmationRequested(
    string UserId,
    string Email,
    string ConfirmationToken);
