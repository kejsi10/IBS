using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.RemoveContact;

/// <summary>
/// Command to remove a contact from a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ContactId">The contact identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record RemoveContactCommand(
    Guid ClientId,
    Guid ContactId,
    Guid TenantId) : ICommand;
