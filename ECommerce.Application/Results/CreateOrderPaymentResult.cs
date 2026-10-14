namespace ECommerce.Application.Results;

public record CreateOrderPaymentResult(
    Guid PaymentId,
    string Status,
    string PaymentUrl);
