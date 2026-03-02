using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.DeactivateUser;

/// <summary>
/// Command to deactivate a user account.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public sealed record DeactivateUserCommand(Guid UserId) : ICommand;
