using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Commissions.Application.Commands.ReconcileLineItem;

/// <summary>
/// Command to reconcile a line item on a commission statement.
/// </summary>
public sealed record ReconcileLineItemCommand(
    Guid TenantId,
    Guid StatementId,
    Guid LineItemId,
    string? ExpectedRowVersion = null
) : ICommand;
