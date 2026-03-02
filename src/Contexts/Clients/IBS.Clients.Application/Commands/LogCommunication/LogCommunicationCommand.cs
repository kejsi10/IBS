using IBS.BuildingBlocks.Application.Commands;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Application.Commands.LogCommunication;

/// <summary>
/// Command to log a communication with a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="UserId">The user logging the communication.</param>
/// <param name="CommunicationType">The type of communication.</param>
/// <param name="Subject">The subject of the communication.</param>
/// <param name="Notes">Additional notes about the communication.</param>
public sealed record LogCommunicationCommand(
    Guid ClientId,
    Guid TenantId,
    Guid UserId,
    CommunicationType CommunicationType,
    string Subject,
    string? Notes) : ICommand<Guid>;
