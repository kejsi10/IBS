using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Claims.Application.Commands.SetReserve;

/// <summary>
/// Command to set a reserve on a claim.
/// </summary>
public sealed record SetReserveCommand(
    Guid TenantId,
    Guid UserId,
    Guid ClaimId,
    string ReserveType,
    decimal Amount,
    string Currency = "USD",
    string? Notes = null,
    string? ExpectedRowVersion = null
) : ICommand;
