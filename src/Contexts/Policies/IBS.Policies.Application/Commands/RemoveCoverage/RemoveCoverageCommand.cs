using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.RemoveCoverage;

/// <summary>
/// Command to remove (deactivate) a coverage from a policy.
/// </summary>
public sealed record RemoveCoverageCommand(
    Guid TenantId,
    Guid PolicyId,
    Guid CoverageId,
    string? ExpectedRowVersion = null
) : ICommand;
