using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.RejectEndorsement;

/// <summary>
/// Command to reject a pending endorsement.
/// </summary>
public sealed record RejectEndorsementCommand(
    Guid TenantId,
    Guid UserId,
    Guid PolicyId,
    Guid EndorsementId,
    string Reason,
    string? ExpectedRowVersion = null
) : ICommand;
