using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface IProductService
{
    Task<Result<IReadOnlyList<GetProductListDto>>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<Result<GetProductDto>> GetByIdAsync(
        int id, 
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<GetSellerProductDto>>> GetAllMineAsync(
        string userId, 
        CancellationToken cancellationToken);

    Task<Result<GetSellerProductDto>> GetMineByIdAsync(
        int id,
        string userId, 
        CancellationToken cancellationToken);

    Task<Result<GetSellerProductDto>> CreateProductAsync(
        string userId,
        CreateProductDto dto,
        CancellationToken cancellationToken);

    Task<Result> UpdateProductAsync(
        int id,
        string userId,
        UpdateProductDto dto, 
        CancellationToken cancellationToken);

    Task<Result> ActivateProductAsync(
        int id, 
        string userId,
        CancellationToken cancellationToken);

    Task<Result> DeactivateProductAsync(
        int id,
        string userId,
        CancellationToken cancellationToken);

    Task<Result> BlockProductAsync(
        int id, 
        CancellationToken cancellationToken);

    Task<Result> UnblockProductAsync(
        int id, 
        CancellationToken cancellationToken);
}
