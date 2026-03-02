using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Identity.Application.Commands.ActivateUser;

/// <summary>
/// Command to activate a user account.
/// </summary>
/// <param name="UserId">The user identifier.</param>
public sealed record ActivateUserCommand(Guid UserId) : ICommand;
