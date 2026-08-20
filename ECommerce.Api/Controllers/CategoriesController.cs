using ECommerce.Application.DTOs.Categories;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController(
       ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetCategoryDto>>> GetAllAsync()
    {
        var result = await categoryService.GetAllAsync();

        return Ok(result.Value);
    }

    [HttpGet("{id:int}", Name = nameof(GetByIdAsync))]
    public async Task<ActionResult<GetCategoryDto>> GetByIdAsync(int id)
    {
        var result = await categoryService.GetByIdAsync(id);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<ActionResult<GetCategoryDto>> CreateAsync(CreateCategoryDto dto)
    {
        var result = await categoryService.CreateAsync(dto);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return CreatedAtRoute(
            nameof(GetByIdAsync),
            new { id = result.Value!.Id },
            result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var result = await categoryService.UpdateAsync(id, dto);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await categoryService.DeleteAsync(id);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }
    private ActionResult MapError(Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound =>
                NotFound(error),

            ErrorType.Conflict =>
                Conflict(error),

            ErrorType.Validation =>
                BadRequest(error),

            ErrorType.Forbidden =>
                StatusCode(StatusCodes.Status403Forbidden, error),

            _ =>
                StatusCode(StatusCodes.Status500InternalServerError, error)
        };
    }
}

