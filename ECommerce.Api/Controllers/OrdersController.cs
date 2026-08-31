using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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
}
