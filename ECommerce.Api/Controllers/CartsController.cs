using ECommerce.Application.DTOs.CartItems;
using ECommerce.Application.DTOs.Carts;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Route("api/carts")]
[Authorize]
[ApiController]
public class CartsController(
    ICartService cartService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GetCartDto>> GetCartAsync(
        CancellationToken cancellationToken)
    {
        var result = await cartService.GetMineAsync(
            CurrentUserId,
            cancellationToken);

        return Ok(result.Value);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddCartItem(
        AddCartItemDto dto,
        CancellationToken cancellationToken)
    {
        var result = await cartService.AddCartItemAsync(
            CurrentUserId,
            dto,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }

    [HttpPatch("items/{id:int}/increment")]
    public async Task<IActionResult> IncrementCartItemAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await cartService.IncrementCartItemAsync(
            id,
            CurrentUserId,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }

    [HttpPatch("items/{id:int}/decrement")]
    public async Task<IActionResult> DecrementCartItemAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await cartService.DecrementCartItemAsync(
            id,
            CurrentUserId,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }

    [HttpDelete("items/{id:int}")]
    public async Task<IActionResult> DeleteCartItemAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await cartService.DeleteCartItemAsync(
            id,
            CurrentUserId,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }
}