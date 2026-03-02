using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Application.Commands.CreateClaim;

/// <summary>
/// Command to create a new claim (FNOL).
/// </summary>
public sealed record CreateClaimCommand(
    Guid TenantId,
    Guid UserId,
    Guid PolicyId,
    Guid ClientId,
    DateTimeOffset LossDate,
    DateTimeOffset ReportedDate,
    LossType LossType,
    string LossDescription,
    decimal? EstimatedLossAmount = null,
    string? EstimatedLossCurrency = "USD"
) : ICommand<Guid>;
