namespace ECommerce.Contracts.Messaging;

public class MessageRoutingKeys
{
    public const string EmailConfirmationRequested =
        "email.confirmation.requested";

    public const string DeadLetterRoutingKey =
        "email.confirmation.failed";

    public const string PaymentSucceeded =
        "payment.succeeded";

    public const string PaymentFailed =
        "payment.failed";
}
