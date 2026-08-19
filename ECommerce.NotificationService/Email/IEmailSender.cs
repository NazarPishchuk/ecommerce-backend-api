namespace ECommerce.NotificationService.Email;

public interface IEmailSender
{
    Task SendConfirmationEmailAsync(
        string email,
        string userId,
        string token,
        CancellationToken cancellationToken = default);
}
