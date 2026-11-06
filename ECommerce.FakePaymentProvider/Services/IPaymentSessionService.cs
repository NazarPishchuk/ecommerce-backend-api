using ECommerce.FakePaymentProvider.DTOs;
using ECommerce.FakePaymentProvider.Entities;

namespace ECommerce.FakePaymentProvider.Services;

public interface IPaymentSessionService
{
    CreatePaymentSessionResultDto? CreateSession(
        CreatePaymentSessionDto dto);

    GetPaymentSessionDto? GetSession(string id);

    Task<SessionCompletionResult> CompleteSessionAsync(
        string id,
        bool succeeded,
        CancellationToken cancellationToken);
}
