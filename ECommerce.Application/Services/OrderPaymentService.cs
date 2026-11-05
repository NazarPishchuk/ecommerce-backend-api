using System.Net;
using System.Net.Http.Json;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;
using ECommerce.Domain.Enums;

namespace ECommerce.Api.Services;

public class OrderPaymentService(
    IOrderRepository orderRepository,
    HttpClient httpClient) : IOrderPaymentService
{
    public async Task<Result<CreateOrderPaymentResult>> CreatePaymentAsync(
        int orderId,
        string userId,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAndUserIdAsync(
            orderId,
            userId,
            cancellationToken);

        if (order is null)
        {
            return Result<CreateOrderPaymentResult>.Failure(
                new Error(
                    "Order.NotFound",
                    ErrorType.NotFound,
                    "Order was not found."));
        }

        if (order.PaymentId.HasValue || order.PaidAtUtc.HasValue)
        {
            return Result<CreateOrderPaymentResult>.Failure(
                new Error(
                    "Order.AlreadyPaid",
                    ErrorType.Conflict,
                    "Order has already been paid."));
        }

        if (order.Status != OrderStatus.Pending)
        {
            return Result<CreateOrderPaymentResult>.Failure(
                new Error(
                    "Order.PaymentNotAllowed",
                    ErrorType.Conflict,
                    "Only pending orders can be paid."));
        }

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

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            return Result<CreateOrderPaymentResult>.Failure(
                new Error(
                    "Payment.Conflict",
                    ErrorType.Conflict,
                    "Payment cannot be created for the current order state."));
        }

        response.EnsureSuccessStatusCode();

        var payment = await response.Content
            .ReadFromJsonAsync<CreateOrderPaymentResult>(
                cancellationToken: cancellationToken);

        if (payment is null)
        {
            throw new InvalidOperationException(
                "PaymentService returned an empty response.");
        }

        return Result<CreateOrderPaymentResult>.Success(payment);
    }
}


