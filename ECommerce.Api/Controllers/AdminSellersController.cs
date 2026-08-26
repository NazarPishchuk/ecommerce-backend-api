using ECommerce.Api.Controllers;
using ECommerce.Application.Authorization;
using ECommerce.Application.DTOs.Sellers;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_API.Controllers;

[Authorize(Roles = AppRoles.Admin)]
[Route("api/admin/sellerprofiles")]
[ApiController]
public class AdminSellersController(ISellerProfileService sellerProfileService) : ApiControllerBase
{
    
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetAdminSellerProfileDto>>>
        GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await sellerProfileService.GetAdminSellerProfileListAsync(
                cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSellerProfileAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sellerProfileService.DeleteSellerProfileByIdAsync(
            id,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }

    [HttpPatch("{id:int}/ban")]
    public async Task<IActionResult> BanSellerAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sellerProfileService.BanSellerAsync(
            id,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }

    [HttpPatch("{id:int}/unban")]
    public async Task<IActionResult> UnbanSellerAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await sellerProfileService.UnbanSellerAsync(
            id,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }
}
