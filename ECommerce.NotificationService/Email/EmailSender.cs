using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace ECommerce.NotificationService.Email;

public sealed class EmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendConfirmationEmailAsync(
        string email,
        string userId,
        string token,
        CancellationToken cancellationToken = default)
    {
        var confirmationUrl =
            $"{_options.ApplicationUrl}/api/auth/confirm-email" +
            $"?userId={Uri.EscapeDataString(userId)}" +
            $"&token={Uri.EscapeDataString(token)}";

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));

        message.To.Add(MailboxAddress.Parse(email));

        message.Subject = "Confirm your email";

        message.Body = new TextPart(TextFormat.Html)
        {
            Text = $"""
                <h2>Confirm your email</h2>

                <p>Thanks for registering!</p>

                <p>
                    Please confirm your email address by clicking
                    the button below.
                </p>

                <p>
                    <a href="{confirmationUrl}"
                       style="
                           display: inline-block;
                           padding: 12px 20px;
                           background-color: #222222;
                           color: #ffffff;
                           text-decoration: none;
                           border-radius: 6px;
                           font-weight: bold;">
                        Confirm Email
                    </a>
                </p>

                <p>
                    If you didn't create this account,
                    you can ignore this email.
                </p>
                """
        };

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(
            _options.Host,
            _options.Port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await smtpClient.AuthenticateAsync(
            _options.UserName,
            _options.Password,
            cancellationToken);

        await smtpClient.SendAsync(
            message,
            cancellationToken);

        await smtpClient.DisconnectAsync(
            quit: true,
            cancellationToken);
    }
}
