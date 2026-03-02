using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.Logout;

/// <summary>
/// Command to logout a user and revoke all refresh tokens.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public sealed record LogoutCommand(Guid UserId) : ICommand;
