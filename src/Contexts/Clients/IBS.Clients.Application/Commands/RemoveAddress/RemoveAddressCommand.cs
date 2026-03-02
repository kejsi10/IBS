using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.RemoveAddress;

/// <summary>
/// Command to remove an address from a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="AddressId">The address identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record RemoveAddressCommand(
    Guid ClientId,
    Guid AddressId,
    Guid TenantId) : ICommand;
