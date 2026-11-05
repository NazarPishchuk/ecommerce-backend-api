using ECommerce.Api.Services;
using ECommerce.Application.DTOs.Orders.Customer;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace ECommerce.Api.Controllers.Orders;

[Route("api/orders")]
[Authorize]
[ApiController]
public class OrdersController(
    IOrderService orderService,
    IOrderPaymentService orderPaymentService) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrderAsync(
        CancellationToken cancellationToken)
    {
        var result = await orderService.CheckoutAsync(
            CurrentUserId, 
            cancellationToken);

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

    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> Pay(
    int id,
    CancellationToken cancellationToken)
    {
        var result = await orderPaymentService.CreatePaymentAsync(
            id,
            CurrentUserId,
            cancellationToken);

        if (result.IsFailure)
            return MapError(result.Error!);

        return Ok(result);
    }
}
