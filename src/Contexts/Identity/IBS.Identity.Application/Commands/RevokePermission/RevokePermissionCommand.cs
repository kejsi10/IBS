using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.RevokePermission;

/// <summary>
/// Command to revoke a permission from a role.
/// </summary>
/// <param name="RoleId">The role identifier.</param>
/// <param name="PermissionId">The permission identifier.</param>
public sealed record RevokePermissionCommand(
    Guid RoleId,
    Guid PermissionId) : ICommand;
