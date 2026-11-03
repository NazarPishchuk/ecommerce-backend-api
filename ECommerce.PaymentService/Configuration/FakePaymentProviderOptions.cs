namespace ECommerce.PaymentService.Configuration;

public sealed class FakePaymentProviderOptions
{
    public const string SectionName = "FakePaymentProvider";

    public string WebhookSecret { get; set; } = string.Empty;
}
