using ECommerce.Application.Authorization;
using ECommerce.Application.DTOs.Orders.Admin;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers.Orders;

[Route("api/admin/orders")]
[Authorize(Roles = AppRoles.Admin)]
[ApiController]
public class AdminOrdersController(
    IOrderService orderService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetAdminOrderListDto>>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var result = await orderService.GetAllForAdminAsync(
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }
}