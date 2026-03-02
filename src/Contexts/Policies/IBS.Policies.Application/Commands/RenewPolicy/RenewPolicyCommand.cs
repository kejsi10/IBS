using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.RenewPolicy;

/// <summary>
/// Command to create a renewal policy from an existing active policy.
/// </summary>
public sealed record RenewPolicyCommand(
    Guid TenantId,
    Guid UserId,
    Guid PolicyId,
    string? ExpectedRowVersion = null
) : ICommand<Guid>;
