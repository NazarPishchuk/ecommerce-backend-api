using ECommerce.FakePaymentProvider.DTOs;
using ECommerce.FakePaymentProvider.Entities;
using ECommerce.FakePaymentProvider.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ECommerce.FakePaymentProvider.Pages;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class PayModel(
    IPaymentSessionService paymentSessionService,
    ILogger<PayModel> logger) : PageModel
{
    public GetPaymentSessionDto PaymentSession { get; private set; } = null!;

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet(string id)
    {
        if (!LoadSession(id))
            return NotFound();

        return Page();
    }

    public Task<IActionResult> OnPostSucceedAsync(
        string id,
        CancellationToken cancellationToken)
    {
        return CompleteAsync(id, true, cancellationToken);
    }

    public Task<IActionResult> OnPostFailAsync(
        string id,
        CancellationToken cancellationToken)
    {
        return CompleteAsync(id, false, cancellationToken);
    }

    public Task<IActionResult> OnPostRetryWebhookAsync(
        string id,
        CancellationToken cancellationToken)
    {
        if (!LoadSession(id))
            return Task.FromResult<IActionResult>(NotFound());

        if (PaymentSession.Status == FakePaymentStatus.Pending)
            return Task.FromResult<IActionResult>(BadRequest());

        var succeeded =
            PaymentSession.Status == FakePaymentStatus.Succeeded;

        return CompleteAsync(id, succeeded, cancellationToken);
    }

    private async Task<IActionResult> CompleteAsync(
        string id,
        bool succeeded,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await paymentSessionService.CompleteSessionAsync(
                id,
                succeeded,
                cancellationToken);

            return result switch
            {
                SessionCompletionResult.NotFound => NotFound(),

                SessionCompletionResult.Conflict => StatusCode(
                    StatusCodes.Status409Conflict,
                    "Payment session already has a different result."),

                SessionCompletionResult.Completed
                    or SessionCompletionResult.AlreadyCompleted =>
                    RedirectToPage("/Pay", new { id }),

                _ => throw new InvalidOperationException(
                    "Unknown session completion result.")
            };
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(
                exception,
                "Webhook delivery failed for session {SessionId}.",
                id);

            return ShowDeliveryError(id);
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                exception,
                "Webhook delivery timed out for session {SessionId}.",
                id);

            return ShowDeliveryError(id);
        }
    }

    private IActionResult ShowDeliveryError(string id)
    {
        if (!LoadSession(id))
            return NotFound();

        ErrorMessage =
            "The simulation result was saved in the fake provider, " +
            "but no acknowledgment was received from PaymentService. " +
            "You can retry sending the result.";

        Response.StatusCode = StatusCodes.Status502BadGateway;

        return Page();
    }

    private bool LoadSession(string id)
    {
        var session = paymentSessionService.GetSession(id);

        if (session is null)
            return false;

        PaymentSession = session;

        return true;
    }
}
