using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ECommerce.PaymentService.Configuration;
using ECommerce.PaymentService.DTOs;
using ECommerce.PaymentService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ECommerce.PaymentService.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController(
    IPaymentService paymentService,
    PaymentWebhookService webhookService,
    IOptions<FakePaymentProviderOptions> options) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    [HttpPost]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreatePaymentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await paymentService.CreatePaymentAsync(
            dto.OrderId,
            dto.Amount,
            dto.Currency,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetPayment),
            new { id = result.PaymentId },
            result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetPaymentDto>> GetPayment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var payment = await paymentService.GetPaymentAsync(
            id,
            cancellationToken);

        if (payment is null)
            return NotFound();

        return Ok(payment);
    }

    [HttpPost("webhook")]
    [RequestSizeLimit(16_384)]
    public async Task<IActionResult> HandleWebhookAsync(
        CancellationToken cancellationToken)
    {
        if (!Request.Headers.TryGetValue(
                "X-FakeProvider-Signature",
                out var signatureHeaders) ||
            signatureHeaders.Count != 1)
        {
            return Unauthorized();
        }

        var signature = signatureHeaders[0];

        if (string.IsNullOrWhiteSpace(signature) || signature.Length != 64)
            return Unauthorized();

        byte[] receivedSignature;

        try
        {
            receivedSignature = Convert.FromHexString(signature);
        }
        catch (FormatException)
        {
            return Unauthorized();
        }

        using var bodyStream = new MemoryStream();

        await Request.Body.CopyToAsync(
            bodyStream,
            cancellationToken);

        var body = bodyStream.ToArray();

        var expectedSignature = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(options.Value.WebhookSecret),
            body);

        if (!CryptographicOperations.FixedTimeEquals(
                expectedSignature,
                receivedSignature))
        {
            return Unauthorized();
        }

        PaymentWebhookDto? dto;

        try
        {
            dto = JsonSerializer.Deserialize<PaymentWebhookDto>(
                body,
                JsonOptions);
        }
        catch (JsonException)
        {
            return BadRequest(new
            {
                Error = "Invalid webhook JSON."
            });
        }

        if (dto is null)
        {
            return BadRequest(new
            {
                Error = "Webhook payload is required."
            });
        }

        var result = await webhookService.HandleAsync(
            dto,
            cancellationToken);

        return result switch
        {
            PaymentWebhookResult.Processed
                or PaymentWebhookResult.AlreadyProcessed =>
                NoContent(),

            PaymentWebhookResult.InvalidPayload =>
                BadRequest(new
                {
                    Error = "Invalid webhook payload."
                }),

            PaymentWebhookResult.NotFound =>
                NotFound(new
                {
                    Error = "Payment was not found."
                }),

            PaymentWebhookResult.Conflict =>
                Conflict(new
                {
                    Error = "Webhook does not match the payment or its final state."
                }),

            _ => throw new InvalidOperationException(
                "Unknown webhook processing result.")
        };
    }
}
