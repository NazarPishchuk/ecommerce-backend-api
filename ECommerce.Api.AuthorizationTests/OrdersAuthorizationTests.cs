using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using ECommerce.Api.Controllers.Orders;
using ECommerce.Api.Services;
using ECommerce.Application.Authorization;
using ECommerce.Application.DTOs.Orders.Customer;
using ECommerce.Application.DTOs.Sellers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Mapping;
using ECommerce.Application.Results;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Identity;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace ECommerce.Api.AuthorizationTests;

public sealed class OrdersAuthorizationTests(OrdersApiFixture fixture)
    : IClassFixture<OrdersApiFixture>
{
    [Theory]
    [InlineData("GET", "/api/orders")]
    [InlineData("GET", "/api/orders/1")]
    [InlineData("POST", "/api/orders/1/pay")]
    [InlineData("PATCH", "/api/orders/1/cancel")]
    [InlineData("POST", "/api/sellerprofiles")]
    public async Task Unauthenticated_requests_return_401(string method, string path)
    {
        using var response = await fixture.SendAsync(method, path);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("seller-only", "GET", "/api/orders")]
    [InlineData("seller-only", "GET", "/api/orders/1")]
    [InlineData("seller-only", "POST", "/api/orders/1/pay")]
    [InlineData("seller-only", "PATCH", "/api/orders/1/cancel")]
    [InlineData("seller-only", "POST", "/api/sellerprofiles")]
    [InlineData("admin-only", "GET", "/api/orders")]
    [InlineData("admin-only", "GET", "/api/orders/1")]
    [InlineData("admin-only", "POST", "/api/orders/1/pay")]
    [InlineData("admin-only", "PATCH", "/api/orders/1/cancel")]
    [InlineData("admin-only", "POST", "/api/sellerprofiles")]
    public async Task Non_customer_roles_get_403(
        string token, string method, string path)
    {
        using var response = await fixture.SendAsync(method, path, token);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Customer_can_list_read_and_pay_own_order()
    {
        using var listResponse = await fixture.SendAsync("GET", "/api/orders", "customer-a");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<GetOrderListDto>>();
        Assert.Equal(fixture.OwnerOrderId, Assert.Single(list!).Id);

        using var detailResponse = await fixture.SendAsync(
            "GET", $"/api/orders/{fixture.OwnerOrderId}", "customer-a");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        var detail = await detailResponse.Content.ReadFromJsonAsync<GetOrderDto>();
        Assert.Equal(fixture.OwnerOrderId, detail!.Id);

        using var payResponse = await fixture.SendAsync(
            "POST", $"/api/orders/{fixture.OwnerOrderId}/pay", "customer-a");
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);
    }

    [Fact]
    public async Task Customer_cannot_list_read_pay_or_cancel_another_customers_order()
    {
        using var listResponse = await fixture.SendAsync("GET", "/api/orders", "customer-a");
        var list = await listResponse.Content.ReadFromJsonAsync<List<GetOrderListDto>>();
        Assert.DoesNotContain(list!, order => order.Id == fixture.OtherOrderId);

        using var detailResponse = await fixture.SendAsync(
            "GET", $"/api/orders/{fixture.OtherOrderId}", "customer-a");
        Assert.Equal(HttpStatusCode.NotFound, detailResponse.StatusCode);

        var paymentCallsBefore = fixture.PaymentCalls;
        using var payResponse = await fixture.SendAsync(
            "POST", $"/api/orders/{fixture.OtherOrderId}/pay", "customer-a");
        Assert.Equal(HttpStatusCode.NotFound, payResponse.StatusCode);
        Assert.Equal(paymentCallsBefore, fixture.PaymentCalls);

        using var cancelResponse = await fixture.SendAsync(
            "PATCH", $"/api/orders/{fixture.OtherOrderId}/cancel", "customer-a");
        Assert.Equal(HttpStatusCode.NotFound, cancelResponse.StatusCode);
    }

    [Fact]
    public async Task Customer_can_cancel_own_order()
    {
        using var response = await fixture.SendAsync(
            "PATCH", $"/api/orders/{fixture.OtherOrderId}/cancel", "customer-b");
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Customer_can_pass_seller_profile_creation_authorization()
    {
        using var body = JsonContent.Create(new
        {
            StoreName = "Test store",
            Description = "Test description"
        });
        using var response = await fixture.SendAsync(
            "POST", "/api/sellerprofiles", "customer-a", body);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Customer_and_seller_can_access_own_order_endpoints()
    {
        using var listResponse = await fixture.SendAsync("GET", "/api/orders", "customer-seller");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var list = await listResponse.Content.ReadFromJsonAsync<List<GetOrderListDto>>();
        Assert.Equal(fixture.OwnerOrderId, Assert.Single(list!).Id);

        using var detailResponse = await fixture.SendAsync(
            "GET", $"/api/orders/{fixture.OwnerOrderId}", "customer-seller");
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);

        using var payResponse = await fixture.SendAsync(
            "POST", $"/api/orders/{fixture.OwnerOrderId}/pay", "customer-seller");
        Assert.Equal(HttpStatusCode.OK, payResponse.StatusCode);
    }
}

public sealed class OrdersApiFixture : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private string _connectionString = null!;
    private readonly PaymentStubHandler _payment = new();

    public int OwnerOrderId { get; private set; }
    public int OtherOrderId { get; private set; }
    public int PaymentCalls => _payment.Calls;

    public async Task InitializeAsync()
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "ECOMMERCE_TEST_SQL_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "Set ECOMMERCE_TEST_SQL_CONNECTION_STRING for isolated SQL integration tests.");
        _connectionString = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = $"ECommerceOrdersAuthTests_{Guid.NewGuid():N}"
        }.ConnectionString;

        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = "Testing"
        });
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Loopback, 0));
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(OrdersController).Assembly);
        builder.Services.AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddDbContext<ECommerceDbContext>(options =>
            options.UseSqlServer(_connectionString));
        builder.Services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<ECommerceDbContext>());
        builder.Services.AddScoped<IOrderRepository, OrderRepository>();
        builder.Services.AddScoped<ICartRepository, CartRepository>();
        builder.Services.AddScoped<ISellerProfileRepository, SellerProfileRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped<IOrderService, OrderService>();
        builder.Services.AddSingleton(
            DispatchProxy.Create<ISellerProfileService, SellerProfileServiceStub>());
        builder.Services.AddScoped<IOrderPaymentService>(sp =>
            new OrderPaymentService(sp.GetRequiredService<IOrderRepository>(),
                new HttpClient(_payment, disposeHandler: false)
                {
                    BaseAddress = new Uri("http://payment-stub/")
                }));
        builder.Services.AddAutoMapper(_ => { }, typeof(OrderMappingProfile));

        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapControllers();

        await using (var scope = _app.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
            await db.Database.EnsureCreatedAsync();
            db.Users.AddRange(
                new ApplicationUser
                {
                    Id = "customer-a", UserName = "customer-a",
                    FirstName = "A", LastName = "Customer"
                },
                new ApplicationUser
                {
                    Id = "customer-b", UserName = "customer-b",
                    FirstName = "B", LastName = "Customer"
                });
            var ownerOrder = new Order
            {
                UserId = "customer-a", Status = OrderStatus.Pending,
                TotalAmount = 10, CreatedAt = DateTimeOffset.UtcNow
            };
            var otherOrder = new Order
            {
                UserId = "customer-b", Status = OrderStatus.Pending,
                TotalAmount = 20, CreatedAt = DateTimeOffset.UtcNow
            };
            otherOrder.SellerOrders.Add(new SellerOrder
            {
                StoreName = "Test store",
                Status = SellerOrderStatus.Pending
            });
            db.Orders.AddRange(ownerOrder, otherOrder);
            await db.SaveChangesAsync();
            OwnerOrderId = ownerOrder.Id;
            OtherOrderId = otherOrder.Id;
        }

        await _app.StartAsync();
        var addresses = _app.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()!.Addresses;
        _client = new HttpClient { BaseAddress = new Uri(Assert.Single(addresses)) };
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
        if (_app is not null)
        {
            await _app.StopAsync();
            await using (var scope = _app.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ECommerceDbContext>();
                await db.Database.EnsureDeletedAsync();
            }
            await _app.DisposeAsync();
        }
        _payment.Dispose();
    }

    public async Task<HttpResponseMessage> SendAsync(
        string method, string path, string? token = null, HttpContent? content = null)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        request.Content = content;
        if (token is not null)
            request.Headers.Authorization = new("Bearer", token);
        return await _client.SendAsync(request);
    }
}

public sealed class TestAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(
        options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var header = Request.Headers.Authorization.ToString();
        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(AuthenticateResult.NoResult());

        var token = header["Bearer ".Length..];
        var identity = token switch
        {
            "customer-a" => (UserId: "customer-a", Roles: new[] { AppRoles.Customer }),
            "customer-b" => (UserId: "customer-b", Roles: new[] { AppRoles.Customer }),
            "customer-seller" => (UserId: "customer-a", Roles: new[] { AppRoles.Customer, AppRoles.Seller }),
            "seller-only" => (UserId: "seller", Roles: new[] { AppRoles.Seller }),
            "admin-only" => (UserId: "admin", Roles: new[] { AppRoles.Admin }),
            _ => (UserId: "", Roles: Array.Empty<string>())
        };
        if (identity.UserId.Length == 0)
            return Task.FromResult(AuthenticateResult.Fail("Unknown test identity."));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, identity.UserId)
        };
        claims.AddRange(identity.Roles.Select(role => new Claim("role", role)));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            claims, Scheme.Name, ClaimTypes.Name, "role"));
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(principal, Scheme.Name)));
    }
}

public class SellerProfileServiceStub : DispatchProxy
{
    protected override object? Invoke(MethodInfo? method, object?[]? args)
    {
        if (method?.Name == nameof(ISellerProfileService.CreateSellerProfileAsync))
        {
            return Task.FromResult(Result<CreateSellerProfileResponse>.Failure(
                new Error("Test.Conflict", ErrorType.Conflict,
                    "Authorization passed; no profile is created in this test.")));
        }

        throw new NotSupportedException("Only profile creation is exercised by this test.");
    }
}

internal sealed class PaymentStubHandler : HttpMessageHandler
{
    private int _calls;
    public int Calls => Volatile.Read(ref _calls);

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _calls);
        var result = new CreateOrderPaymentResult(
            Guid.NewGuid(), "Pending", "http://payment-stub/checkout");
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(result), Encoding.UTF8, "application/json")
        });
    }
}
