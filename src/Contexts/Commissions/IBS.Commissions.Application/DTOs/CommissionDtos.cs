using IBS.BuildingBlocks.Domain;

namespace IBS.Commissions.Application.DTOs;

/// <summary>
/// Full commission schedule DTO.
/// </summary>
public sealed record CommissionScheduleDto(
    Guid Id,
    Guid CarrierId,
    string CarrierName,
    string LineOfBusiness,
    decimal NewBusinessRate,
    decimal RenewalRate,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

/// <summary>
/// Commission schedule list item DTO.
/// </summary>
public sealed record CommissionScheduleListItemDto(
    Guid Id,
    Guid CarrierId,
    string CarrierName,
    string LineOfBusiness,
    decimal NewBusinessRate,
    decimal RenewalRate,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Full commission statement DTO with line items and splits.
/// </summary>
public sealed record CommissionStatementDto(
    Guid Id,
    Guid CarrierId,
    string CarrierName,
    string StatementNumber,
    int PeriodMonth,
    int PeriodYear,
    DateOnly StatementDate,
    string Status,
    decimal TotalPremium,
    string TotalPremiumCurrency,
    decimal TotalCommission,
    string TotalCommissionCurrency,
    DateTimeOffset ReceivedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<CommissionLineItemDto> LineItems,
    IReadOnlyList<ProducerSplitDto> ProducerSplits,
    string RowVersion
) : IConcurrencyAware;

/// <summary>
/// Statement list item DTO.
/// </summary>
public sealed record CommissionStatementListItemDto(
    Guid Id,
    Guid CarrierId,
    string CarrierName,
    string StatementNumber,
    int PeriodMonth,
    int PeriodYear,
    DateOnly StatementDate,
    string Status,
    decimal TotalPremium,
    string TotalPremiumCurrency,
    decimal TotalCommission,
    string TotalCommissionCurrency,
    int LineItemCount,
    int ReconciledCount,
    int DisputedCount,
    DateTimeOffset ReceivedAt,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Commission line item DTO.
/// </summary>
public sealed record CommissionLineItemDto(
    Guid Id,
    Guid? PolicyId,
    string PolicyNumber,
    string InsuredName,
    string LineOfBusiness,
    DateOnly EffectiveDate,
    string TransactionType,
    decimal GrossPremium,
    string GrossPremiumCurrency,
    decimal CommissionRate,
    decimal CommissionAmount,
    string CommissionAmountCurrency,
    bool IsReconciled,
    DateTimeOffset? ReconciledAt,
    string? DisputeReason
);

/// <summary>
/// Producer split DTO.
/// </summary>
public sealed record ProducerSplitDto(
    Guid Id,
    Guid LineItemId,
    string ProducerName,
    Guid ProducerId,
    decimal SplitPercentage,
    decimal SplitAmount,
    string SplitAmountCurrency
);

/// <summary>
/// Commission statistics DTO.
/// </summary>
public sealed record CommissionStatisticsDto(
    int TotalStatements,
    int ReceivedStatements,
    int ReconcilingStatements,
    int ReconciledStatements,
    int PaidStatements,
    int DisputedStatements,
    decimal TotalCommissionAmount,
    decimal TotalPaidAmount,
    decimal TotalDisputedAmount,
    Dictionary<string, int> StatementsByStatus,
    Dictionary<string, decimal> CommissionByCarrier
);

/// <summary>
/// Commission summary report entry DTO.
/// </summary>
public sealed record CommissionSummaryEntryDto(
    Guid CarrierId,
    string CarrierName,
    int PeriodMonth,
    int PeriodYear,
    int StatementCount,
    decimal TotalPremium,
    decimal TotalCommission,
    decimal TotalPaid,
    string Currency
);

/// <summary>
/// Producer report entry DTO.
/// </summary>
public sealed record ProducerReportEntryDto(
    Guid ProducerId,
    string ProducerName,
    int PeriodMonth,
    int PeriodYear,
    int LineItemCount,
    decimal TotalSplitAmount,
    decimal AverageSplitPercentage,
    string Currency
);
