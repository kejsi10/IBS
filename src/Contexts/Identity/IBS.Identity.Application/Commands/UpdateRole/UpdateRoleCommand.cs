using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.UpdateRole;

/// <summary>
/// Command to update an existing role.
/// </summary>
/// <param name="RoleId">The role identifier.</param>
/// <param name="TenantId">The tenant identifier (for uniqueness check).</param>
/// <param name="Name">The new role name.</param>
/// <param name="Description">The new role description (optional).</param>
public sealed record UpdateRoleCommand(
    Guid RoleId,
    Guid TenantId,
    string Name,
    string? Description = null) : ICommand;
