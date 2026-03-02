using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.AddCoverage;

/// <summary>
/// Command to add a coverage to a policy.
/// </summary>
public sealed record AddCoverageCommand(
    Guid TenantId,
    Guid PolicyId,
    string Code,
    string Name,
    decimal PremiumAmount,
    string? Description = null,
    decimal? LimitAmount = null,
    decimal? DeductibleAmount = null,
    bool IsOptional = false,
    string? ExpectedRowVersion = null
) : ICommand<Guid>;
