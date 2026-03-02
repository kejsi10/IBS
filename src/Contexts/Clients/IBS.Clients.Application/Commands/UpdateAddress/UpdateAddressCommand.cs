using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Clients.Application.Commands.UpdateAddress;

/// <summary>
/// Command to update an address.
/// </summary>
/// <param name="ClientId">The client identifier.</param>
/// <param name="AddressId">The address identifier.</param>
/// <param name="TenantId">The tenant identifier.</param>
/// <param name="StreetLine1">The first line of the street address.</param>
/// <param name="StreetLine2">The second line of the street address.</param>
/// <param name="City">The city.</param>
/// <param name="State">The state/province.</param>
/// <param name="PostalCode">The postal/zip code.</param>
/// <param name="Country">The country.</param>
public sealed record UpdateAddressCommand(
    Guid ClientId,
    Guid AddressId,
    Guid TenantId,
    string StreetLine1,
    string? StreetLine2,
    string City,
    string State,
    string PostalCode,
    string Country) : ICommand;
