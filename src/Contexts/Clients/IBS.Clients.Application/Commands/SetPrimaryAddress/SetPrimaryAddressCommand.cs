using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.SetPrimaryAddress;

/// <summary>
/// Command to set an address as the primary address for its type.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="AddressId">The address identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record SetPrimaryAddressCommand(
    Guid ClientId,
    Guid AddressId,
    Guid TenantId) : ICommand;
