using ECommerce.Application.Results;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ECommerce.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult MapError(Error error)
    {
        return error.Type switch
        {
            ErrorType.NotFound =>
                NotFound(error),

            ErrorType.Conflict =>
                Conflict(error),

            ErrorType.Validation =>
                BadRequest(error),

            ErrorType.Unauthorized =>
                Unauthorized(error),

            ErrorType.Forbidden =>
                StatusCode(StatusCodes.Status403Forbidden, error),

            _ =>
                StatusCode(
                    StatusCodes.Status500InternalServerError,
                    error)
        };
    }

    protected string CurrentUserId =>
        User.FindFirstValue(JwtRegisteredClaimNames.Sub)
        ?? throw new InvalidOperationException(
            "Authenticated user doess not contain a 'sub' claim.");
}
