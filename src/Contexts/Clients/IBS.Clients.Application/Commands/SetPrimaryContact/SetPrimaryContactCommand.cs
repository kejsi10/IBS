using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.SetPrimaryContact;

/// <summary>
/// Command to set a contact as the primary contact for a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="ContactId">The contact identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record SetPrimaryContactCommand(
    Guid ClientId,
    Guid ContactId,
    Guid TenantId) : ICommand;
