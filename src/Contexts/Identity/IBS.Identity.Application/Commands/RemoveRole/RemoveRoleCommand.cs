using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.RemoveRole;

/// <summary>
/// Command to remove a role from a user.
/// </summary>
/// <param name="UserId">The user identifier.</param>
/// <param name="RoleId">The role identifier.</param>
public sealed record RemoveRoleCommand(
    Guid UserId,
    Guid RoleId) : ICommand;
