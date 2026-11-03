namespace ECommerce.PaymentService.Services;

public enum PaymentWebhookResult
{
    Processed,
    AlreadyProcessed,
    InvalidPayload,
    NotFound,
    Conflict
}
