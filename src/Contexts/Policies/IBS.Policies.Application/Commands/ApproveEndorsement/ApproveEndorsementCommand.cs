using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.ApproveEndorsement;

/// <summary>
/// Command to approve a pending endorsement.
/// </summary>
public sealed record ApproveEndorsementCommand(
    Guid TenantId,
    Guid UserId,
    Guid PolicyId,
    Guid EndorsementId,
    string? ExpectedRowVersion = null
) : ICommand;
