using ECommerce.Api.Controllers;
using ECommerce.Application.Authorization;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController(IProductService productService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetProductListDto>>> GetProductsAllAsync(
        CancellationToken cancellationToken)
    {
        var result = await productService.GetAllAsync(cancellationToken);

        return Ok(result.Value);
    }


    [HttpGet("{id:int}", Name = nameof(GetProductByIdAsync))]
    public async Task<ActionResult<GetProductDto>> GetProductByIdAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var result = await productService.GetByIdAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }


    [Authorize(Roles = AppRoles.Seller)]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<GetSellerProductDto>>> GetAllMineProductsAsync(
        CancellationToken cancellationToken)
    {
        var result = await productService.GetAllMineAsync(CurrentUserId, cancellationToken);

        return Ok(result.Value);
    }


    [Authorize(Roles = AppRoles.Seller)]
    [HttpGet("{id:int}/mine", Name = nameof(GetMineProductByIdAsync))]
    public async Task<ActionResult<GetSellerProductDto>> GetMineProductByIdAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var result = await productService.GetMineByIdAsync(id, CurrentUserId, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return Ok(result.Value);
    }


    [Authorize(Roles = AppRoles.Seller)]
    [HttpPost]
    public async Task<ActionResult<GetSellerProductDto>> CreateProductAsync(
        CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        var result = await productService.CreateProductAsync(CurrentUserId, dto, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return CreatedAtRoute(
            nameof(GetProductByIdAsync),
            new { id = result.Value!.Id },
            result.Value);
    }
    

    [Authorize(Roles = AppRoles.Seller)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProductAsync(
        int id, 
        UpdateProductDto dto, 
        CancellationToken cancellationToken)
    {
        var result = await productService.UpdateProductAsync(id, CurrentUserId, dto, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }


    [Authorize(Roles = AppRoles.Seller)]
    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> ActivateProductAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var result = await productService.ActivateProductAsync(id, CurrentUserId, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }


    [Authorize(Roles = AppRoles.Seller)]
    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> DeactivateProductAsync(
        int id,
        CancellationToken cancellationToken) 
    {
        var result = await productService.DeactivateProductAsync(id, CurrentUserId, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }


    [Authorize(Roles = AppRoles.Admin)]
    [HttpPatch("{id:int}/block")]
    public async Task<IActionResult> BlockProductAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var result = await productService.BlockProductAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }


    [Authorize(Roles = AppRoles.Admin)]
    [HttpPatch("{id:int}/unblock")]
    public async Task<IActionResult> UnblockProductAsync(
        int id, 
        CancellationToken cancellationToken)
    {
        var result = await productService.UnblockProductAsync(id, cancellationToken);

        if (result.IsFailure)
        {
            return MapError(result.Error!);
        }

        return NoContent();
    }
}
