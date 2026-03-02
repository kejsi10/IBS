using IBS.Identity.Application.Commands.CreateRole;
using IBS.Identity.Application.Commands.GrantPermission;
using IBS.Identity.Application.Commands.RevokePermission;
using IBS.Identity.Application.Commands.UpdateRole;
using IBS.Identity.Application.Queries.GetPermissions;
using IBS.Identity.Application.Queries.GetRoleById;
using IBS.Identity.Application.Queries.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IBS.Api.Controllers;

/// <summary>
/// Controller for managing roles and permissions.
/// </summary>
[Authorize]
public sealed class RolesController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RolesController> _logger;

    /// <summary>
    /// Initializes a new instance of the RolesController class.
    /// </summary>
    /// <param name="mediator">The mediator.</param>
    /// <param name="logger">The logger.</param>
    public RolesController(IMediator mediator, ILogger<RolesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets all roles available to the current tenant.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of roles.</returns>
    /// <response code="200">Returns the list of roles.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting roles for tenant {TenantId}", CurrentTenantId);

        var query = new GetRolesQuery(CurrentTenantId);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a role by identifier.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The role details with permissions.</returns>
    /// <response code="200">Returns the role.</response>
    /// <response code="404">If the role is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("{id:guid}", Name = "GetRoleById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRole(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting role {RoleId}", id);

        var query = new GetRoleByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new tenant-specific role.
    /// </summary>
    /// <param name="request">The create role request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created role's identifier.</returns>
    /// <response code="201">Returns the newly created role ID.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="409">If a role with the same name already exists.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating role {RoleName} for tenant {TenantId}", request.Name, CurrentTenantId);

        var command = new CreateRoleCommand(CurrentTenantId, request.Name, request.Description);
        var result = await _mediator.Send(command, cancellationToken);

        return ToCreatedResult(result, "GetRoleById", new { id = result.IsSuccess ? result.Value : Guid.Empty });
    }

    /// <summary>
    /// Updates an existing role.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="request">The update role request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the role was updated successfully.</response>
    /// <response code="400">If the request is invalid or role is a system role.</response>
    /// <response code="404">If the role is not found.</response>
    /// <response code="409">If a role with the same name already exists.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateRole(
        Guid id,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating role {RoleId}", id);

        var command = new UpdateRoleCommand(id, CurrentTenantId, request.Name, request.Description);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Grants a permission to a role.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="request">The grant permission request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the permission was granted successfully.</response>
    /// <response code="404">If the role or permission is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpPost("{id:guid}/permissions")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GrantPermission(
        Guid id,
        [FromBody] GrantPermissionRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Granting permission {PermissionId} to role {RoleId}", request.PermissionId, id);

        var command = new GrantPermissionCommand(id, request.PermissionId);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Revokes a permission from a role.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="permissionId">The permission identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    /// <response code="204">If the permission was revoked successfully.</response>
    /// <response code="404">If the role is not found.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpDelete("{id:guid}/permissions/{permissionId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RevokePermission(
        Guid id,
        Guid permissionId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Revoking permission {PermissionId} from role {RoleId}", permissionId, id);

        var command = new RevokePermissionCommand(id, permissionId);
        var result = await _mediator.Send(command, cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gets all available permissions, optionally filtered by module.
    /// </summary>
    /// <param name="module">Optional module filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of permissions.</returns>
    /// <response code="200">Returns the list of permissions.</response>
    /// <response code="401">If the user is not authenticated.</response>
    [HttpGet("permissions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPermissions(
        [FromQuery] string? module,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting permissions with module filter: {Module}", module);

        var query = new GetPermissionsQuery(module);
        var result = await _mediator.Send(query, cancellationToken);

        return ToActionResult(result);
    }
}

/// <summary>
/// Request model for creating a role.
/// </summary>
public sealed record CreateRoleRequest
{
    /// <summary>
    /// Gets the role name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the role description (optional).
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// Request model for updating a role.
/// </summary>
public sealed record UpdateRoleRequest
{
    /// <summary>
    /// Gets the role name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the role description (optional).
    /// </summary>
    public string? Description { get; init; }
}

/// <summary>
/// Request model for granting a permission to a role.
/// </summary>
public sealed record GrantPermissionRequest
{
    /// <summary>
    /// Gets the permission identifier.
    /// </summary>
    public Guid PermissionId { get; init; }
}
