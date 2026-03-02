using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Commissions.Application.Commands.DisputeLineItem;

/// <summary>
/// Command to dispute a line item on a commission statement.
/// </summary>
public sealed record DisputeLineItemCommand(
    Guid TenantId,
    Guid StatementId,
    Guid LineItemId,
    string Reason,
    string? ExpectedRowVersion = null
) : ICommand;
