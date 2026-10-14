using ECommerce.FakePaymentProvider.Entities;

namespace ECommerce.FakePaymentProvider.DTOs;

public record GetPaymentSessionDto(
    string Id,
    decimal Amount,
    string Currency,
    FakePaymentStatus Status);
