using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.ActivatePolicy;

/// <summary>
/// Command to activate (issue) a bound policy.
/// </summary>
public sealed record ActivatePolicyCommand(
    Guid TenantId,
    Guid PolicyId,
    string? ExpectedRowVersion = null
) : ICommand;
