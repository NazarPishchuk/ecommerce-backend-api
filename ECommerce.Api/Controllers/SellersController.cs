using ECommerce.Application.Authorization;
using ECommerce.Application.DTOs.Authentication;
using ECommerce.Application.DTOs.Sellers;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Route("api/sellerprofiles")]
[ApiController]
public class SellersController(ISellerProfileService sellerProfileService) : ApiControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetSellerProfileByIdAsync))]
    public async Task<ActionResult<GetSellerProfileDto>> GetSellerProfileByIdAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var result = await sellerProfileService.GetSellerProfileByIdAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }


    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetSellerProfileListDto>>> GetSellerProfileListAsync(
        CancellationToken cancellationToken)
    {
        var result = await sellerProfileService.GetSellerProfileListAsync(cancellationToken);

        return Ok(result.Value);
    }

    [Authorize(Roles = AppRoles.Seller)]
    [HttpGet("mine")]
    public async Task<ActionResult<GetSellerProfileDto>> GetMineSellerProfileAsync(
        CancellationToken cancellationToken)
    {
        var result = await sellerProfileService.GetMineSellerProfileAsync(CurrentUserId, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<CreateSellerProfileResponse>> CreateSellerProfileAsync(
        CreateSellerProfileDto dto, 
        CancellationToken cancellationToken)
    {
        var result = await sellerProfileService.CreateSellerProfileAsync(
            CurrentUserId, dto, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return CreatedAtRoute(
            nameof(GetSellerProfileByIdAsync),
            new { id = result.Value!.SellerProfile.Id },
            result.Value);
    }

    [Authorize(Roles = AppRoles.Seller)]
    [HttpPut]
    public async Task<IActionResult> UpdateSellerProfileAsync(
        UpdateSellerProfileDto dto,
        CancellationToken cancellationToken)
    {
        var result = await sellerProfileService.UpdateSellerProfileAsync(
            CurrentUserId,
            dto,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }

    [Authorize(Roles = AppRoles.Seller)]
    [HttpDelete]
    public async Task<ActionResult<LoginResponse>> DeleteMineSellerProfileAsync(
        CancellationToken cancellationToken)
    {
        var result = await sellerProfileService.DeleteMineSellerProfileAsync(
            CurrentUserId,
            cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }

    [Authorize(Roles = AppRoles.Admin)]
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
}
