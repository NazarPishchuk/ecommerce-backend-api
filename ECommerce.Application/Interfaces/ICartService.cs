using ECommerce.Application.DTOs.CartItems;
using ECommerce.Application.DTOs.Carts;
using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface ICartService
{
    Task<Result<GetCartDto>> GetMineAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<Result> AddCartItemAsync(
        string userid,
        AddCartItemDto dto,
        CancellationToken cancellationToken);
}
