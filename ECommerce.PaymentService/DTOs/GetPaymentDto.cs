using ECommerce.PaymentService.Domain;

namespace ECommerce.PaymentService.DTOs;

public record GetPaymentDto(
    Guid PaymentId,
    int OrderId,
    decimal Amount,
    string Currency,
    PaymentStatus Status,
    DateTime CreatedAt,
    DateTime? PaidAt);
