using ECommerce.Application.Interfaces;
using ECommerce.Application.Mapping;
using ECommerce.Application.Services;
using ECommerce.Infrastructure.Authentication;
using ECommerce.Infrastructure.Identity;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Persistence.Outbox;
using ECommerce.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ECommerce.Infrastructure.Persistence.Seeding;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters
                .Add(new JsonStringEnumConverter());
        });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ECommerceDbConnectionString"))
           .UseSeeding((context, _) =>
           {
               IdentityDataSeeder.Seed(context);
           })
        .UseAsyncSeeding(
            async (context, _, cancellationToken) =>
            {
                await IdentityDataSeeder.SeedAsync(
                    context,
                    cancellationToken);
            }));



builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<IUnitOfWork>(sp =>
    sp.GetRequiredService<ECommerceDbContext>());

builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IOutboxWriter, OutboxWriter>();

builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

builder.Services.AddScoped<ICategoryService, CategoryService>();



builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT configuration is missing.");

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedEmail = true;

        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ECommerceDbContext>()
    .AddDefaultTokenProviders();


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtOptions.Key)),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                RoleClaimType = "role"
            };
    });

builder.Services.AddAuthorization();


builder.Services.AddAutoMapper(
    cfg => { },
    typeof(CategoryMappingProfile));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
