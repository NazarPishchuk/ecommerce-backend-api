using AutoMapper;
using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using System.Transactions;

namespace ECommerce.Application.Services;

public class OrderService(
    ICartRepository cartRepository,
    ISellerProfileRepository sellerProfileRepository,
    IUnitOfWork unitOfWork,
    IProductRepository productRepository,
    IOrderRepository orderRepository,
    IMapper mapper) : IOrderService
{
    public async Task<Result> CheckoutAsync(
        string userId,
        CancellationToken cancellationToken)
    {

        // cart retrieval 
        var cart = await cartRepository.GetByUserIdWithItemsAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            throw new InvalidOperationException(
                $"Cart was not found for user '{userId}'.");
        }

        // chechking if cart
        // is not empty
        if (cart.CartItems.Count == 0)
        {
            return Result.Failure(new Error(
                "Order.Failure",
                ErrorType.Failure,
                "Cart has no items."));
        }

        //checking the products availability 
        foreach (var item in cart.CartItems)
        {
            var product = item.Product;

            if (item.Product is null
                || !item.Product.IsActive
                || item.Product.IsBlocked
                || item.Product.SellerProfileId is null)
            {
                return Result.Failure(new Error(
                    "CartItem.Failure",
                    ErrorType.Failure,
                    "Product is not available."));
            }

            if (product.Quantity < item.Quantity)
            {
                return Result.Failure(new Error(
                    "Order.InsufficientStock",
                    ErrorType.Conflict,
                    $"There is not enough stock for product '{product.Name}'."));
            }
        }

        //grouping cart items by sellers
        var sellerGroups = cart.CartItems
            .GroupBy(x => x.Product.SellerProfileId!.Value)
            .ToList();

        //creating the main order
        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            TotalAmount = 0,
        };

        decimal totalAmount = 0;

        foreach (var sellerGroup in sellerGroups)
        {
            var sellerProfileId = sellerGroup.Key;

            var sellerProfile =
                await sellerProfileRepository.GetPublicByIdAsync(
                    sellerProfileId,
                    cancellationToken);

            if (sellerProfile is null)
            {
                return Result.Failure(new Error(
                    "Order.SellerUnavailable",
                    ErrorType.Conflict,
                    "One of the sellers is currently unavailable."));
            }

            var sellerOrder = new SellerOrder
            {
                SellerProfileId = sellerProfile.Id,
                StoreName = sellerProfile.StoreName,
                Status = SellerOrderStatus.Pending,
                Order = order
            };

            foreach (var cartItem in sellerGroup)
            {
                var product = cartItem.Product;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = cartItem.Quantity,
                    SellerOrder = sellerOrder
                };

                sellerOrder.OrderItems.Add(orderItem);

                totalAmount += product.Price * cartItem.Quantity;
            }

            order.SellerOrders.Add(sellerOrder);
        }

        order.TotalAmount = totalAmount;

        await using var transaction =
               await unitOfWork.BeginTransactionAsync(
                   cancellationToken);

        foreach (var cartItem in cart.CartItems)
        {
            var stockUpdated =
                await productRepository.TryDecreaseStockAsync(
                    cartItem.ProductId,
                    cartItem.Quantity,
                    cancellationToken);

            if (!stockUpdated)
            {
                return Result.Failure(new Error(
                "Order.CheckoutConflict",
                ErrorType.Conflict,
                "One or more products changed during checkout. " +
                "Please refresh your cart and try again."));
            }
        }

        orderRepository.Add(order);

        await cartRepository.DeleteAllItemsAsync(
            cart.Id,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<GetOrderListDto>>> GetMineAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetByUserIdAsync(
            userId, 
            cancellationToken);

        var dto = mapper.Map<IReadOnlyList<GetOrderListDto>>(orders);

        return Result<IReadOnlyList<GetOrderListDto>>.Success(dto);
    }

}
