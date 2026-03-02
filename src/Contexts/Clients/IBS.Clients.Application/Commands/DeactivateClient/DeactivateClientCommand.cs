using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.DeactivateClient;

/// <summary>
/// Command to deactivate a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
public sealed record DeactivateClientCommand(Guid ClientId) : ICommand;
