using IBS.Identity.Application.Commands.ActivateUser;
using IBS.Identity.Application.Commands.AssignRole;
using IBS.Identity.Application.Commands.DeactivateUser;
using IBS.Identity.Application.Commands.RemoveRole;
using IBS.Identity.Application.Commands.UpdateUserProfile;
using IBS.Identity.Application.Queries.GetUserById;
using IBS.Identity.Application.Queries.SearchUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for managing users.
/// </summary>
[Authorize]
public sealed class UsersController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of the UsersController class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Searches users within the current tenant.
    /// </summary>
    /// <param name="search">Optional search term for name or email.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of users.</returns>
    /// <response code="200">Returns the list of users.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching users for tenant {TenantId}", CurrentTenantId);

        var query = new SearchUsersQuery(CurrentTenantId, search, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a user by identifier.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user details.</returns>
    /// <response code="200">Returns the user.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("{id:guid}", Name = "GetUserById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting user {UserId}", id);

        var query = new GetUserByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Updates a user's profile information.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The update profile request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the profile was updated successfully.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user profile {UserId}", id);

        var command = new UpdateUserProfileCommand(
            id, request.FirstName, request.LastName, request.Title, request.PhoneNumber);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Activates a user account.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the user was activated successfully.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ActivateUser(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating user {UserId}", id);

        var command = new ActivateUserCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Deactivates a user account.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the user was deactivated successfully.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateUser(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating user {UserId}", id);

        var command = new DeactivateUserCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="request">The assign role request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the role was assigned successfully.</response>
    /// <response code="404">If the user or role is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/roles")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignRole(
        Guid id,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Assigning role {RoleId} to user {UserId}", request.RoleId, id);

        var command = new AssignRoleCommand(id, request.RoleId);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the role was removed successfully.</response>
    /// <response code="404">If the user is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpDelete("{id:guid}/roles/{roleId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveRole(
        Guid id,
        Guid roleId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing role {RoleId} from user {UserId}", roleId, id);

        var command = new RemoveRoleCommand(id, roleId);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }
}

/// <summary>
/// Request model for updating a user's profile.
/// </summary>
public sealed record UpdateUserProfileRequest
{
    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the title (optional).
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets the phone number (optional).
    /// </summary>
    public string? PhoneNumber { get; init; }
}

/// <summary>
/// Request model for assigning a role to a user.
/// </summary>
public sealed record AssignRoleRequest
{
    /// <summary>
    /// Gets the role identifier.
    /// </summary>
    public Guid RoleId { get; init; }
}
