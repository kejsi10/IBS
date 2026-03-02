using IBS.BuildingBlocks.Application;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Base controller for all API controllers.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>
    /// Gets the current user's identifier.
    /// </summary>
    protected Guid CurrentUserId
    {
        get
        {
            var claim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null && Guid.TryParse(claim.Value, out var userId)
                ? userId
                : Guid.Empty;
        }
    }

    /// <summary>
    /// Gets the expected row version from the If-Match header for optimistic concurrency.
    /// Returns null if no If-Match header is present.
    /// </summary>
    protected string? ExpectedRowVersion =>
        Request.Headers.TryGetValue("If-Match", out var ifMatch) && !string.IsNullOrEmpty(ifMatch)
            ? ifMatch.ToString().Trim('"')
            : null;

    /// <summary>
    /// Gets the current tenant identifier.
    /// </summary>
    protected Guid CurrentTenantId
    {
        get
        {
            var claim = User.FindFirst("tenant_id") ?? User.FindFirst("tid");
            return claim != null && Guid.TryParse(claim.Value, out var tenantId)
                ? tenantId
                : Guid.Empty;
        }
    }

    /// <summary>
    /// Converts a Result to an IActionResult.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An appropriate IActionResult.</returns>
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return ToErrorResult(result.Error);
    }

    /// <summary>
    /// Converts a Result to an IActionResult.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>An appropriate IActionResult.</returns>
    protected IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return ToErrorResult(result.Error);
    }

    /// <summary>
    /// Converts a Result&lt;Guid&gt; to a 201 Created response with an <c>{ "id": "..." }</c> body.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <param name="routeName">The route name for the location header.</param>
    /// <param name="routeValues">The route values for the location header.</param>
    /// <returns>An appropriate IActionResult.</returns>
    protected IActionResult ToCreatedResult(Result<Guid> result, string routeName, object routeValues)
    {
        if (result.IsSuccess)
        {
            return CreatedAtRoute(routeName, routeValues, new { id = result.Value });
        }

        return ToErrorResult(result.Error);
    }

    /// <summary>
    /// Converts a Result to an IActionResult with a created status.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <param name="routeName">The route name for the location header.</param>
    /// <param name="routeValues">The route values for the location header.</param>
    /// <returns>An appropriate IActionResult.</returns>
    protected IActionResult ToCreatedResult<T>(Result<T> result, string routeName, object routeValues)
    {
        if (result.IsSuccess)
        {
            return CreatedAtRoute(routeName, routeValues, result.Value);
        }

        return ToErrorResult(result.Error);
    }

    private IActionResult ToErrorResult(Error error)
    {
        if (error.Code.EndsWith(".NotFound") || error.Code == "NotFound")
        {
            return NotFound(new { error = error.Message });
        }

        if (error.Code.Contains("Validation") || error.Code == "Validation.Error")
        {
            if (error is ValidationError validationError)
            {
                return BadRequest(new { error = error.Message, errors = validationError.Errors });
            }
            return BadRequest(new { error = error.Message });
        }

        if (error.Code.Contains("Conflict") || error.Code == "Conflict.Error")
        {
            return Conflict(new { error = error.Message });
        }

        if (error.Code == "Unauthorized")
        {
            return Unauthorized(new { error = error.Message });
        }

        if (error.Code == "Forbidden")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = error.Message });
        }

        return BadRequest(new { error = error.Message });
    }
}
