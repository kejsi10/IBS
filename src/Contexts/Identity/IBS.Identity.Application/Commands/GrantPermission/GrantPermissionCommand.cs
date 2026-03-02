using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.GrantPermission;

/// <summary>
/// Command to grant a permission to a role.
/// </summary>
/// <param name="RoleId">The role identifier.</param>
/// <param name="PermissionId">The permission identifier.</param>
public sealed record GrantPermissionCommand(
    Guid RoleId,
    Guid PermissionId) : ICommand;
