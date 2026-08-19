using AutoMapper;
using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Results;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Services;

public class CategoryService(ICategoryRepository categoryRepository,
                            IUnitOfWork unitOfWork,
                            IMapper mapper) : ICategoryService
{
    public async Task<Result<GetCategoryDto>> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        var category = mapper.Map<Category>(dto);

        if(await categoryRepository
            .ExistsByNormalizedNameAsync(category.NormalizedName, cancellationToken))
        {
            return Result<GetCategoryDto>.Failure(new Error(
                "Category.AlreadyExists",
                ErrorType.Conflict,
                "Category with this name already exists."));
        }

        categoryRepository.Add(category);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var categoryDto = mapper.Map<GetCategoryDto>(category);

        return Result<GetCategoryDto>.Success(categoryDto);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken);

        if(category is null)
        {
            return Result.Failure(
                new Error(
                    "Category.NotFound",
                    ErrorType.NotFound,
                    "Category with this id was not found."));
        }

        if(await categoryRepository.HasProductsAsync(id, cancellationToken))
        {
            return Result.Failure(
                new Error(
                    "Category.HasProducts",
                    ErrorType.Conflict,
                    "Category cannot be deleted because it contains products."));
        }

        categoryRepository.Delete(category);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<GetCategoryDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);

        var categoryDtos = mapper.Map<IReadOnlyList<GetCategoryDto>>(categories);

        return Result<IReadOnlyList<GetCategoryDto>>.Success(categoryDtos);
    }

    public async Task<Result<GetCategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken);

        if(category is null)
        {
            return Result<GetCategoryDto>.Failure(
                new Error(
                    "Category.NotFound",
                    ErrorType.NotFound,
                    "Category with this id was not found.")
                );
        }

        var categoryDto = mapper.Map<GetCategoryDto>(category);

        return Result<GetCategoryDto>.Success(categoryDto);
    }

    public async Task<Result> UpdateAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(id, cancellationToken);
        
        if(category is null)
        {
            return Result.Failure(new Error(
                "Category.NotFound",
                ErrorType.NotFound,
                "Category with this id was not found."));
        }

        if (await categoryRepository.ExistsByNormalizedNameAsync(dto.Name.Trim().ToUpperInvariant(), cancellationToken))
        {
            return Result.Failure(new Error(
                "Category.AlreadyExists",
                ErrorType.Conflict,
                "Category with this id alredy exists."));
        }

        mapper.Map(dto, category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
