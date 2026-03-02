using IBS.BuildingBlocks.Application.Commands;
using IBS.Policies.Domain.Events;

namespace IBS.Policies.Application.Commands.CancelPolicy;

/// <summary>
/// Command to cancel a policy.
/// </summary>
public sealed record CancelPolicyCommand(
    Guid TenantId,
    Guid PolicyId,
    DateOnly CancellationDate,
    string Reason,
    CancellationType CancellationType,
    string? ExpectedRowVersion = null
) : ICommand;
