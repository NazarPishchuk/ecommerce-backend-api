using ECommerce.PaymentService.DTOs;

namespace ECommerce.PaymentService.Services;

public enum PaymentCreationStatus
{
    Created,
    AlreadyPaid,
    Reused,
    Conflict
}

public sealed record PaymentCreationResult(
    PaymentCreationStatus Status,
    CreatePaymentResultDto? Payment);
