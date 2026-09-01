using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs.Orders;


namespace ECommerce.Api.Controllers;

[Route("api/orders")]
[ApiController]
public class OrdersController(
    IOrderService orderService) : ApiControllerBase
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync(CancellationToken cancellationToken)
    {
        var result = await orderService.CheckoutAsync(CurrentUserId, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Created();
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetOrderListDto>>> GetMineAsync(
    CancellationToken cancellationToken)
    {
        var result = await orderService.GetMineAsync(
            CurrentUserId,
            cancellationToken);

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetOrderDto>> GetMineByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await orderService.GetMineByIdAsync(
            id,
            CurrentUserId,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }

    [HttpPatch("{id:int}/cancel")]
    public async Task<IActionResult> CancelAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await orderService.CancelAsync(
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
