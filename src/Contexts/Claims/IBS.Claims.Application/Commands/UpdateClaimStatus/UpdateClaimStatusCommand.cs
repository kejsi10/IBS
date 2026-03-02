using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Application.Commands.UpdateClaimStatus;

/// <summary>
/// Command to update the status of a claim.
/// </summary>
public sealed record UpdateClaimStatusCommand(
    Guid TenantId,
    Guid UserId,
    Guid ClaimId,
    ClaimStatus NewStatus,
    decimal? ClaimAmount = null,
    string? ClaimAmountCurrency = "USD",
    string? DenialReason = null,
    string? ClosureReason = null,
    string? AdjusterId = null,
    string? ExpectedRowVersion = null
) : ICommand;
