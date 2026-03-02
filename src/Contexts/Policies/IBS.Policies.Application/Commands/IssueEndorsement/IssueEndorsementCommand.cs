using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Policies.Application.Commands.IssueEndorsement;

/// <summary>
/// Command to issue an approved endorsement and apply premium changes.
/// </summary>
public sealed record IssueEndorsementCommand(
    Guid TenantId,
    Guid PolicyId,
    Guid EndorsementId,
    string? ExpectedRowVersion = null
) : ICommand;
