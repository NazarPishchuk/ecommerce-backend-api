using ECommerce.Application.DTOs.CartItems;
using ECommerce.Application.DTOs.Carts;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services;

public class CartService(
    ICartRepository cartRepository,
    ISellerProfileRepository sellerProfileRepository) : ICartService
{
    public async Task<Result<GetCartDto>> GetMineAsync(
        string userId, 
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdWithItemsAsync(
            userId, 
            cancellationToken);

        if(cart is null)
        {
            throw new InvalidOperationException(
                $"Cart was not found for user '{userId}'.");
        }

        List<int> sellerIds = new();

        foreach(var item in cart.CartItems)
        {
            if (item.Product.SellerProfileId is int sellerProfileId)
            {
                sellerIds.Add(sellerProfileId);
            }
        }

        IReadOnlySet<int> bannedSellerIds =
            await sellerProfileRepository.GetBannedSellerProfileIdsAsync(
            sellerIds, cancellationToken);

        List<GetCartItemDto> cartItems = [];

        int totalItems = 0;

        decimal totalPrice = 0;

        foreach (var item in cart.CartItems)
        {
            totalItems += item.Quantity;
            
            var product = item.Product;

            CartItemAvailabilityStatus status;

            if (!product.IsActive || product.IsBlocked
                || product.SellerProfileId is null ||
                bannedSellerIds.Contains(product.SellerProfileId.Value))
            {
                status = CartItemAvailabilityStatus.Unavailable;
            }
            else if(product.Quantity == 0)
            {
                status = CartItemAvailabilityStatus.OutOfStock;
            }
            else if(product.Quantity < item.Quantity)
            {
                status = CartItemAvailabilityStatus.InsufficientStock;
            }
            else
            {
                status = CartItemAvailabilityStatus.Available;
            }

            if (status == CartItemAvailabilityStatus.Available)
            {
                totalPrice += product.Price * item.Quantity;
            }

            cartItems.Add(new GetCartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Price = item.Product.Price,
                Quantity = item.Quantity,
                AvailableQuantity = item.Product.Quantity,
                AvailabilityStatus = status
            });

        }

        GetCartDto result = new GetCartDto
        {
            Items = cartItems,
            TotalItems = totalItems,
            TotalPrice = totalPrice
        };

        return Result<GetCartDto>.Success(result);
    }
}

