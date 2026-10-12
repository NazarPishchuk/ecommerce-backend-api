using ECommerce.PaymentService.Persistence;
using ECommerce.PaymentService.Providers;
using ECommerce.PaymentService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PaymentDbConnectionString")));

builder.Services.AddScoped<IPaymentProvider, FakePaymentProvider>();

builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
