namespace ECommerce.Application.Messaging;

public sealed record EmailConfirmationRequested(
    string UserId,
    string Email,
    string ConfirmationToken);
