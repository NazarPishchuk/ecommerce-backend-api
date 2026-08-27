using ECommerce.Application.DTOs.CartItems;
using ECommerce.Application.DTOs.Carts;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;


namespace ECommerce.Application.Services;

public class CartService(
    ICartRepository cartRepository,
    ISellerProfileRepository sellerProfileRepository,
    IProductRepository productRepository) : ICartService
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

    public async Task<Result> AddCartItemAsync(
        string userId,
        AddCartItemDto dto,
        CancellationToken cancellationToken)
    {

        //checking cart existence
        var cart = await cartRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        if (cart is null)
        {
            throw new InvalidOperationException(
                $"Cart was not found for user '{userId}'.");
        }

        //checking product existence
        var product = await productRepository.GetByIdAsync(
            dto.ProductId, 
            cancellationToken);

        if(product is null)
        {
            return Result.Failure(new Error(
                "Product.NotFound",
                ErrorType.NotFound,
                "Product with this id was not found."));
        }

        // CartItem may already exist.
        // Try to increase its quantity atomically.
        var itemUpdated =
            await cartRepository.TryIncreaseItemQuantityAsync(
                cart.Id,
                product.Id,
                dto.Quantity,
                cancellationToken);

        if (itemUpdated)
        {
            return Result.Success();
        }

        // Atomic update returned false:
        // either the item does not exist,
        // or there is not enough stock.
        var existingItem = await cartRepository.GetItemByProductIdAsync(
            cart.Id, 
            product.Id, 
            cancellationToken);

        if (existingItem is not null)
        {
            return Result.Failure(new Error(
                "Cart.InsufficientStock",
                ErrorType.Conflict,
                "The requested quantity exceeds the available stock."));
        }

        // The item does not exist yet.
        if (dto.Quantity > product.Quantity)
        {
            return Result.Failure(new Error(
                "Cart.InsufficientStock",
                ErrorType.Conflict,
                "The requested quantity exceeds the available stock."));
        }

        //creation
        var newCartItem = new CartItem
        {
            CartId = cart.Id,
            ProductId = product.Id,
            Quantity = dto.Quantity
        };

        var itemAdded = await cartRepository.TryAddItemAsync(
            newCartItem,
            cancellationToken);

        if (itemAdded)
        {
            return Result.Success();
        }

        // Another parallel request created the same CartItem
        // before our INSERT was completed.
        // Apply our quantity to the item it created.

        itemUpdated = await cartRepository.TryIncreaseItemQuantityAsync(
            cart.Id,
            product.Id,
            dto.Quantity,
            cancellationToken);

        if (itemUpdated)
        {
            return Result.Success();
        }

        return Result.Failure(new Error(
            "Cart.InsufficientStock",
            ErrorType.Conflict,
            "The requested quantity exceeds the available stock."));
    }
}

