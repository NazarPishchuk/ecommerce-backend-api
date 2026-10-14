using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ECommerce.FakePaymentProvider.DTOs;
using ECommerce.FakePaymentProvider.Entities;

namespace ECommerce.FakePaymentProvider.Services;

public class PaymentWebhookSender(
    HttpClient httpClient,
    IConfiguration configuration) : IPaymentWebhookSender
{
    public async Task SendAsync(
        PaymentSession session,
        CancellationToken cancellationToken)
    {
        var webhookUrl =
            configuration["FakePaymentProvider:PaymentServiceWebhookUrl"]
            ?? throw new InvalidOperationException(
                "PaymentService webhook URL is not configured.");

        var secret =
            configuration["FakePaymentProvider:WebhookSecret"]
            ?? throw new InvalidOperationException(
                "Webhook secret is not configured.");

        var dto = new PaymentWebhookDto(
            session.Id,
            session.PaymentId,
            session.Status.ToString(),
            session.PaidAt);

        var json = JsonSerializer.Serialize(
            dto,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        var signatureBytes = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(secret),
            Encoding.UTF8.GetBytes(json));

        var signature = Convert.ToHexString(signatureBytes);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            webhookUrl);

        request.Headers.Add(
            "X-FakeProvider-Signature",
            signature);

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
