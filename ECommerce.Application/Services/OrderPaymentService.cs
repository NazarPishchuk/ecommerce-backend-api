using ECommerce.Application.Interfaces;
using System.Net.Http.Json;
using ECommerce.Application.Results;

namespace ECommerce.Api.Services;

public class OrderPaymentService(
    IOrderRepository orderRepository,
    HttpClient httpClient) : IOrderPaymentService
{
    public async Task<CreateOrderPaymentResult?> CreatePaymentAsync(
        int orderId,
        string userId,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAndUserIdAsync(
            orderId,
            userId,
            cancellationToken);

        if (order is null)
            return null;

        var request = new
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            Currency = "EUR" 
        };

        using var response = await httpClient.PostAsJsonAsync(
            "api/payments",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<CreateOrderPaymentResult>(
                cancellationToken);
    }
}



