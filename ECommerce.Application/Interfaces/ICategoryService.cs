using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Results;

namespace ECommerce.Application.Interfaces;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<GetCategoryDto>>> GetAllAsync();
    Task<Result<GetCategoryDto>> GetByIdAsync(int id);
    Task<Result<GetCategoryDto>> CreateAsync(CreateCategoryDto dto);
    Task<Result> UpdateAsync(int id, UpdateCategoryDto dto);
    Task<Result> DeleteAsync(int id);
}
