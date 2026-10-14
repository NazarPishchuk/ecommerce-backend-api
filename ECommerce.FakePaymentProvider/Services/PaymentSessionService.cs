using ECommerce.FakePaymentProvider.DTOs;
using ECommerce.FakePaymentProvider.Entities;

namespace ECommerce.FakePaymentProvider.Services;

public class PaymentSessionService(
    IPaymentSessionStore sessionStore,
    IPaymentWebhookSender webhookSender,
    IConfiguration configuration) : IPaymentSessionService
{
    public CreatePaymentSessionResultDto CreateSession(
        CreatePaymentSessionDto dto)
    {
        var baseUrl =
            configuration["FakePaymentProvider:PublicBaseUrl"]
            ?? throw new InvalidOperationException(
                "PublicBaseUrl is not configured.");

        var session = new PaymentSession
        {
            PaymentId = dto.PaymentId,
            Amount = dto.Amount,
            Currency = dto.Currency.ToUpperInvariant()
        };

        sessionStore.Add(session);

        var paymentUrl =
            $"{baseUrl.TrimEnd('/')}/pay/{session.Id}";

        return new CreatePaymentSessionResultDto(
            session.Id,
            paymentUrl);
    }

    public GetPaymentSessionDto? GetSession(string id)
    {
        var session = sessionStore.GetById(id);

        if (session is null)
            return null;

        return new GetPaymentSessionDto(
            session.Id,
            session.Amount,
            session.Currency,
            session.Status);
    }

    public async Task<SessionCompletionResult> CompleteSessionAsync(
        string id,
        bool succeeded,
        CancellationToken cancellationToken)
    {
        var session = sessionStore.GetById(id);

        if (session is null)
            return SessionCompletionResult.NotFound;

        var targetStatus = succeeded
            ? FakePaymentStatus.Succeeded
            : FakePaymentStatus.Failed;

        SessionCompletionResult result;

        lock (session)
        {
            if (session.Status != FakePaymentStatus.Pending)
            {
                if (session.Status != targetStatus)
                    return SessionCompletionResult.Conflict;

                result = SessionCompletionResult.AlreadyCompleted;
            }
            else
            {
                session.Status = targetStatus;

                if (succeeded)
                    session.PaidAt = DateTime.UtcNow;

                result = SessionCompletionResult.Completed;
            }
        }

        await webhookSender.SendAsync(
            session,
            cancellationToken);

        return result;
    }
}
