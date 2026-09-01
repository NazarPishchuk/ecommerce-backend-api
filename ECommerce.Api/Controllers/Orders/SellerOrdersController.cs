using ECommerce.Api.Controllers;
using ECommerce.Application.Authorization;
using ECommerce.Application.DTOs.Orders.Seller;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/seller/orders")]
[Authorize(Roles = AppRoles.Seller)]
[ApiController]
public class SellerOrdersController(
    IOrderService orderService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetSellerOrderListDto>>> GetMineAsync(
        CancellationToken cancellationToken)
    {
        var result = await orderService.GetMineSellerOrdersAsync(
            CurrentUserId,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GetSellerOrderDetailsDto>> GetMineByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await orderService.GetMineSellerOrderByIdAsync(
            id,
            CurrentUserId,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }

    [HttpPatch("{id:int}/confirm")]
    public async Task<IActionResult> ConfirmAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await orderService.ConfirmSellerOrderAsync(
            id,
            CurrentUserId,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }

    [HttpPatch("{id:int}/reject")]
    public async Task<IActionResult> RejectAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await orderService.RejectSellerOrderAsync(
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
