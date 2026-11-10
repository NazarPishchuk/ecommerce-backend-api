using ECommerce.NotificationService.Email;
using ECommerce.NotificationService.Messaging;
using ECommerce.NotificationService.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<NotificationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString(
        "NotificationDbConnectionString")
        ?? throw new InvalidOperationException(
            "Notification database connection string is not configured.");

    options.UseSqlServer(connectionString);
});

builder.Services
    .AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.HostName),
        "RabbitMQ HostName is required.")
    .Validate(
        options => options.Port is > 0 and <= 65535,
        "RabbitMQ Port must be valid.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.UserName),
        "RabbitMQ UserName is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Password),
        "RabbitMQ Password is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ExchangeName),
        "RabbitMQ ExchangeName is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.QueueName),
        "RabbitMQ QueueName is required.")
    .ValidateOnStart();

builder.Services
    .AddOptions<EmailOptions>()
    .Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Host),
        "Email Host is required.")
    .Validate(
        options => options.Port is > 0 and <= 65535,
        "Email Port must be valid.")
    .Validate(
        options => !options.UseAuthentication || !string.IsNullOrWhiteSpace(options.UserName),
        "Email UserName is required when authentication is enabled.")
    .Validate(
        options => !options.UseAuthentication || !string.IsNullOrWhiteSpace(options.Password),
        "Email Password is required when authentication is enabled.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.FromEmail),
        "Email FromEmail is required.")
    .ValidateOnStart();

builder.Services.AddHostedService<EmailConfirmationRequestedConsumer>();

builder.Services.AddSingleton<IEmailSender, EmailSender>();

var host = builder.Build();

if (builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await using var scope = host.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<NotificationDbContext>().Database;
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
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

await host.RunAsync();
