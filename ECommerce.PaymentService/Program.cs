using ECommerce.PaymentService.Persistence;
using ECommerce.PaymentService.Providers;
using ECommerce.PaymentService.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using ECommerce.PaymentService.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddScoped<PaymentWebhookService>();

builder.Services
    .AddOptions<FakePaymentProviderOptions>()
    .Bind(builder.Configuration.GetSection(
        FakePaymentProviderOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.WebhookSecret),
        "Fake payment provider webhook secret is required.")
    .ValidateOnStart();

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PaymentDbConnectionString")));

builder.Services.AddScoped<IPaymentProvider, FakePaymentProvider>();

builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddHttpClient<
    IPaymentProvider,
    FakePaymentProvider>(client =>
    {
        var baseUrl = builder.Configuration["FakePaymentProvider:BaseUrl"]
            ?? throw new InvalidOperationException(
                "FakePaymentProvider BaseUrl is not configured.");

        client.BaseAddress = new Uri(baseUrl);
    });

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
