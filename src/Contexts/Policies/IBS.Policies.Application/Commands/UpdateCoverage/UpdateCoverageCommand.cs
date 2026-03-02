using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.UpdateCoverage;

/// <summary>
/// Command to update coverage details on a policy.
/// </summary>
public sealed record UpdateCoverageCommand(
    Guid TenantId,
    Guid PolicyId,
    Guid CoverageId,
    string Name,
    decimal PremiumAmount,
    string? Description = null,
    decimal? LimitAmount = null,
    decimal? PerOccurrenceLimit = null,
    decimal? AggregateLimit = null,
    decimal? DeductibleAmount = null,
    string? ExpectedRowVersion = null
) : ICommand;
