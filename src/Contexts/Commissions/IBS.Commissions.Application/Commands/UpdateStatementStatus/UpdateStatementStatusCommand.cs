using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Application.Commands.UpdateStatementStatus;

/// <summary>
/// Command to update the status of a commission statement.
/// </summary>
public sealed record UpdateStatementStatusCommand(
    Guid TenantId,
    Guid StatementId,
    StatementStatus NewStatus,
    string? ExpectedRowVersion = null
) : ICommand;
