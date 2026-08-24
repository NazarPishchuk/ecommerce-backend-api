using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ECommerce.Api.Controllers;

[Route("api/categories")]
[ApiController]

public class CategoriesController(
       ICategoryService categoryService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetCategoryDto>>> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        var result = await categoryService.GetAllAsync(cancellationToken);

        return Ok(result.Value);
    }

    [HttpGet("{id:int}", Name = nameof(GetCategoryByIdAsync))]
    public async Task<ActionResult<GetCategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await categoryService.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<ActionResult<GetCategoryDto>> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken)
    {
        var result = await categoryService.CreateAsync(dto, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return CreatedAtRoute(
            nameof(GetCategoryByIdAsync),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> UpdateCategoryAsync(int id, UpdateCategoryDto dto, CancellationToken cancellationToken)
    {
        var result = await categoryService.UpdateAsync(id, dto, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> DeleteCategoryAsync(int id, CancellationToken cancellationToken)
    {
        var result = await categoryService.DeleteAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }
}

