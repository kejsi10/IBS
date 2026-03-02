using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Claims.Application.Commands.AddClaimNote;

/// <summary>
/// Command to add a note to a claim.
/// </summary>
public sealed record AddClaimNoteCommand(
    Guid TenantId,
    Guid UserId,
    Guid ClaimId,
    string Content,
    bool IsInternal = false,
    string? ExpectedRowVersion = null
) : ICommand;
