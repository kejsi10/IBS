using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.ReinstatePolicy;

/// <summary>
/// Command to reinstate a cancelled policy.
/// </summary>
public sealed record ReinstatePolicyCommand(
    Guid TenantId,
    Guid PolicyId,
    string Reason,
    string? ExpectedRowVersion = null
) : ICommand;
