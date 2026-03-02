using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.BindPolicy;

/// <summary>
/// Command to bind a policy.
/// </summary>
public sealed record BindPolicyCommand(
    Guid TenantId,
    Guid UserId,
    Guid PolicyId,
    string? ExpectedRowVersion = null
) : ICommand;
