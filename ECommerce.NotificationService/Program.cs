using ECommerce.NotificationService.Email;
using ECommerce.NotificationService.Messaging;

var builder = Host.CreateApplicationBuilder(args);

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
        options => !string.IsNullOrWhiteSpace(options.UserName),
        "Email UserName is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.Password),
        "Email Password is required.")
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.FromEmail),
        "Email FromEmail is required.")
    .ValidateOnStart();

builder.Services.AddHostedService<EmailConfirmationRequestedConsumer>();

builder.Services.AddSingleton<IEmailSender, EmailSender>();

var host = builder.Build();

await host.RunAsync();