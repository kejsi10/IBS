using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.AssignRole;

/// <summary>
/// Command to assign a role to a user.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="RoleId">The role identifier.</param>
public sealed record AssignRoleCommand(
    Guid UserId,
    Guid RoleId) : ICommand;
