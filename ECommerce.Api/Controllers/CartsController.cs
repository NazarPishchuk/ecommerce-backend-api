using ECommerce.Application.DTOs.Carts;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Route("api/carts")]
[ApiController]
public class CartsController(
    ICartService cartService) : ApiControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<GetCartDto>> GetCarAsync(
        CancellationToken cancellationToken)
    {
        var result = await cartService.GetMineAsync(
            CurrentUserId, 
            cancellationToken);

        return Ok(result.Value);
    }
}
