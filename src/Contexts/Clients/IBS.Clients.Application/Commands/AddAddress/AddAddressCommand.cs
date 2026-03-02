using IBS.BuildingBlocks.Application.Commands;
using IBS.Clients.Domain.ValueObjects;

namespace IBS.Clients.Application.Commands.AddAddress;

/// <summary>
/// Command to add an address to a client.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="AddressType">The type of address.</param>
/// <param name="StreetLine1">The first line of the street address.</param>
/// <param name="StreetLine2">The second line of the street address.</param>
/// <param name="City">The city.</param>
/// <param name="State">The state/province.</param>
/// <param name="PostalCode">The postal/zip code.</param>
/// <param name="Country">The country.</param>
/// <param name="IsPrimary">Whether this is the primary address for its type.</param>
public sealed record AddAddressCommand(
    Guid ClientId,
    Guid TenantId,
    AddressType AddressType,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsPrimary) : ICommand<Guid>;
