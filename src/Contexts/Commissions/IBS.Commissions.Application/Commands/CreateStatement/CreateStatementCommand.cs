using IBS.BuildingBlocks.Application.Commands;

namespace IBS.Commissions.Application.Commands.CreateStatement;

/// <summary>
/// Command to create a new commission statement.
/// </summary>
public sealed record CreateStatementCommand(
    Guid TenantId,
    Guid CarrierId,
    string CarrierName,
    string StatementNumber,
    int PeriodMonth,
    int PeriodYear,
    DateOnly StatementDate,
    decimal TotalPremium,
    string TotalPremiumCurrency,
    decimal TotalCommission,
    string TotalCommissionCurrency
) : ICommand<Guid>;
