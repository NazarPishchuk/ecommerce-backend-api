using AutoMapper;
using ECommerce.Application.DTOs.Orders.Admin;
using ECommerce.Application.DTOs.Orders.Customer;
using ECommerce.Application.DTOs.Orders.Seller;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

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

        var cartConsumed = await cartRepository.TryConsumeItemsAsync(
            cart.Id,
            cart.CartItems.ToList(),
            cancellationToken);

        if (!cartConsumed)
        {
            return Result.Failure(new Error(
                "Order.CheckoutConflict",
                ErrorType.Conflict,
                "Cart changed during checkout. Please refresh it and try again."));
        }

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

    public async Task<Result<GetOrderDto>> GetMineByIdAsync(
        int id,
        string userId,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAndUserIdAsync(
            id,
            userId,
            cancellationToken);

        if (order is null)
        {
            return Result<GetOrderDto>.Failure(new Error(
                "Order.NotFound",
                ErrorType.NotFound,
                "Order with this id was not found."));
        }

        var dto = mapper.Map<GetOrderDto>(order);

        return Result<GetOrderDto>.Success(dto);
    }

    public async Task<Result> CancelAsync(
        int id,
        string userId,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAndUserIdForUpdateAsync(
            id,
            userId,
            cancellationToken);

        if (order is null)
        {
            return Result.Failure(new Error(
                "Order.NotFound",
                ErrorType.NotFound,
                "Order with this id was not found."));
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(
            cancellationToken);

        var cancelled =
            await orderRepository.TryCancelSellerOrdersAsync(
                id,
                userId,
                cancellationToken);

        if (!cancelled)
        {
            return Result.Failure(new Error(
                "SellerOrder.Conflict",
                ErrorType.Conflict,
                "At least one seller order is no longer pending."));
        }

        order.Status = OrderStatus.Cancelled;

        foreach (var sellerOrder in order.SellerOrders)
        {
            foreach(var orderItem in sellerOrder.OrderItems)
            {
                await productRepository.IncreaseStockAsync(
                    orderItem.ProductId, 
                    orderItem.Quantity,
                    cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<GetSellerOrderListDto>>> GetMineSellerOrdersAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var sellerOrders =
            await orderRepository.GetSellerOrdersByUserIdAsync(
                userId,
                cancellationToken);

        var dto = mapper.Map<IReadOnlyList<GetSellerOrderListDto>>(
            sellerOrders);

        return Result<IReadOnlyList<GetSellerOrderListDto>>.Success(dto);
    }

    public async Task<Result<GetSellerOrderDetailsDto>> GetMineSellerOrderByIdAsync(
        int id,
        string userId,
        CancellationToken cancellationToken)
    {
        var sellerOrder =
            await orderRepository.GetSellerOrderByIdAndUserIdAsync(
                id,
                userId,
                cancellationToken);

        if (sellerOrder is null)
        {
            return Result<GetSellerOrderDetailsDto>.Failure(new Error(
                "SellerOrder.NotFound",
                ErrorType.NotFound,
                "Seller order with this id was not found."));
        }

        var dto = mapper.Map<GetSellerOrderDetailsDto>(
            sellerOrder);

        return Result<GetSellerOrderDetailsDto>.Success(dto);
    }

    public async Task<Result> ConfirmSellerOrderAsync(
        int id,
        string userId,
        CancellationToken cancellationToken)
    {
        var sellerOrder =
            await orderRepository.GetSellerOrderByIdAndUserIdAsync(
                id,
                userId,
                cancellationToken);

        if (sellerOrder is null)
        {
            return Result.Failure(new Error(
                "SellerOrder.NotFound",
                ErrorType.NotFound,
                "Seller order with this id was not found."));
        }

        if (sellerOrder.Order.PaymentId is null ||
            sellerOrder.Order.PaidAtUtc is null)
        {
            return Result.Failure(new Error(
                "SellerOrder.PaymentRequired",
                ErrorType.Conflict,
                "Payment has not been successfully reconciled yet."));
        }

        if (sellerOrder.Status != SellerOrderStatus.Pending)
        {
            return Result.Failure(new Error(
                "SellerOrder.Conflict",
                ErrorType.Conflict,
                "Seller order is no longer pending."));
        }

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(
                cancellationToken);

        var confirmed =
            await orderRepository.TryConfirmSellerOrderAsync(
                id,
                userId,
                cancellationToken);

        if (!confirmed)
        {
            return Result.Failure(new Error(
                "SellerOrder.Conflict",
                ErrorType.Conflict,
                "Seller order is no longer pending."));
        }

        await orderRepository.RecalculateOrderStatusAsync(
            sellerOrder.OrderId,
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectSellerOrderAsync(
        int id,
        string userId,
        CancellationToken cancellationToken)
    {
        var sellerOrder =
            await orderRepository.GetSellerOrderByIdAndUserIdAsync(
                id,
                userId,
                cancellationToken);

        if (sellerOrder is null)
        {
            return Result.Failure(new Error(
                "SellerOrder.NotFound",
                ErrorType.NotFound,
                "Seller order with this id was not found."));
        }

        if (sellerOrder.Status != SellerOrderStatus.Pending)
        {
            return Result.Failure(new Error(
                "SellerOrder.Conflict",
                ErrorType.Conflict,
                "Seller order is no longer pending."));
        }

        await using var transaction =
            await unitOfWork.BeginTransactionAsync(
                cancellationToken);

        var rejected = await orderRepository.TryRejectSellerOrderAsync(
            id, 
            userId, 
            cancellationToken);

        if (!rejected)
        {
            return Result.Failure(new Error(
                "SellerOrder.Conflict",
                ErrorType.Conflict,
                "Seller order is no longer pending."));
        }

        foreach(var orderItem in sellerOrder.OrderItems)
        {
            await productRepository.IncreaseStockAsync(
                orderItem.ProductId,
                orderItem.Quantity,
                cancellationToken);
        }

        await orderRepository.RecalculateOrderStatusAsync(
            sellerOrder.OrderId,
            cancellationToken);

        await transaction.CommitAsync(
            cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<GetAdminOrderListDto>>> GetAllForAdminAsync(
        CancellationToken cancellationToken)
    {
        var orders = await orderRepository.GetAllAsync(
            cancellationToken);

        var dto = mapper.Map<IReadOnlyList<GetAdminOrderListDto>>(
            orders);

        return Result<IReadOnlyList<GetAdminOrderListDto>>.Success(dto);
    }

    public async Task<Result<GetAdminOrderDto>> GetByIdForAdminAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (order is null)
        {
            return Result<GetAdminOrderDto>.Failure(new Error(
                "Order.NotFound",
                ErrorType.NotFound,
                "Order with this id was not found."));
        }

        var dto = mapper.Map<GetAdminOrderDto>(order);

        return Result<GetAdminOrderDto>.Success(dto);
    }
}
