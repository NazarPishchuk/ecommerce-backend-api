namespace ECommerce.Infrastructure.Messaging.Payments;

public enum PaymentEventHandlingResult
{
    Processed,
    AlreadyProcessed,
    InvalidPayload,
    OrderNotFound,
    Conflict
}
