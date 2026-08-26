using ECommerce.Application.DTOs.Carts;
using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface ICartService
{
    Task<Result<GetCartDto>> GetMineAsync(
        string userId,
        CancellationToken cancellationToken);
}
