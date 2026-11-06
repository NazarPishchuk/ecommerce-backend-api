using ECommerce.PaymentService.Domain;

namespace ECommerce.PaymentService.DTOs;

public record CreatePaymentResultDto(
    Guid PaymentId,
    PaymentStatus Status,
    string PaymentUrl);
