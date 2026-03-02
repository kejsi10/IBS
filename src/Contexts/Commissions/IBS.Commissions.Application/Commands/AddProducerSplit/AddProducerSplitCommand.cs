using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Commissions.Application.Commands.AddProducerSplit;

/// <summary>
/// Command to add a producer split to a line item.
/// </summary>
public sealed record AddProducerSplitCommand(
    Guid TenantId,
    Guid StatementId,
    Guid LineItemId,
    string ProducerName,
    Guid ProducerId,
    decimal SplitPercentage,
    decimal SplitAmount,
    string SplitAmountCurrency
) : ICommand<Guid>;
