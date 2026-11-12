using ECommerce.PaymentService.Configuration;
using ECommerce.PaymentService.Messaging;
using ECommerce.PaymentService.Persistence;
using ECommerce.PaymentService.Persistence.Outbox;
using ECommerce.PaymentService.Providers;
using ECommerce.PaymentService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var internalApiKey = builder.Configuration["PaymentService:InternalApiKey"];
if (string.IsNullOrWhiteSpace(internalApiKey) || internalApiKey.Length < 32)
    throw new InvalidOperationException("PaymentService internal API key must be at least 32 characters.");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddScoped<PaymentWebhookService>();

builder.Services.AddHostedService<OutboxProcessor>();

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

builder.Services
    .AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(
        RabbitMqOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.HostName),
        "RabbitMQ host name is required.")
    .Validate(
        options => options.Port is > 0 and <= 65535,
        "RabbitMQ port is invalid.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.UserName),
        "RabbitMQ user name is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Password),
        "RabbitMQ password is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ExchangeName),
        "RabbitMQ exchange name is required.")
    .ValidateOnStart();

builder.Services.AddSingleton<RabbitMqPublisher>();

builder.Services.AddHostedService<OutboxProcessor>();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<PaymentDbContext>().Database;
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            await database.MigrateAsync();
            break;
        }
        catch (SqlException exception) when (
            attempt < 12 && exception.Number is 1801 or 4060 or 53 or 233 or 40 or 2 or -2)
        {
            app.Logger.LogWarning(exception,
                "SQL Server is still starting. Retrying payment migrations in 5 seconds.");
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "PaymentService API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
