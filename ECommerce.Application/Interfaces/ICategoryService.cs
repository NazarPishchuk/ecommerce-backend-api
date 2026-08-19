using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<GetCategoryDto>>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<GetCategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<GetCategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}
