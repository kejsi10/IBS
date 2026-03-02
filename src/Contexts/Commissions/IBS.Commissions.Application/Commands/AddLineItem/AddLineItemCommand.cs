using IBS.BuildingBlocks.Application.Commands;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Application.Commands.AddLineItem;

/// <summary>
/// Command to add a line item to a commission statement.
/// </summary>
public sealed record AddLineItemCommand(
    Guid TenantId,
    Guid StatementId,
    string PolicyNumber,
    string InsuredName,
    string LineOfBusiness,
    DateOnly EffectiveDate,
    TransactionType TransactionType,
    decimal GrossPremium,
    string GrossPremiumCurrency,
    decimal CommissionRate,
    decimal CommissionAmount,
    string CommissionAmountCurrency,
    Guid? PolicyId = null,
    string? ExpectedRowVersion = null
) : ICommand<Guid>;
