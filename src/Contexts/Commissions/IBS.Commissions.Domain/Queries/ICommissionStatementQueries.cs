using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Commissions.Domain.ValueObjects;

namespace IBS.Commissions.Domain.Queries;

/// <summary>
/// Read-side query interface for commission statements.
/// </summary>
public interface ICommissionStatementQueries
{
    /// <summary>
    /// Gets a statement by its identifier for read operations.
    /// </summary>
    /// <param name="id">The statement identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The statement read model if found.</returns>
    Task<CommissionStatementReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches statements with filtering and pagination.
    /// </summary>
    /// <param name="filter">The search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The search result with pagination.</returns>
    Task<StatementSearchResult> SearchAsync(StatementSearchFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets commission statistics for the dashboard.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Commission statistics.</returns>
    Task<CommissionStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a commission summary report by carrier and period.
    /// </summary>
    /// <param name="carrierId">Optional carrier filter.</param>
    /// <param name="periodMonth">Optional period month filter.</param>
    /// <param name="periodYear">Optional period year filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The summary report entries.</returns>
    Task<IReadOnlyList<CommissionSummaryEntry>> GetSummaryReportAsync(
        Guid? carrierId = null,
        int? periodMonth = null,
        int? periodYear = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a producer report by producer and period.
    /// </summary>
    /// <param name="producerId">Optional producer filter.</param>
    /// <param name="periodMonth">Optional period month filter.</param>
    /// <param name="periodYear">Optional period year filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The producer report entries.</returns>
    Task<IReadOnlyList<ProducerReportEntry>> GetProducerReportAsync(
        Guid? producerId = null,
        int? periodMonth = null,
        int? periodYear = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Read model for a commission statement.
/// </summary>
public sealed record CommissionStatementReadModel(
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
    IReadOnlyList<LineItemReadModel> LineItems,
    IReadOnlyList<ProducerSplitReadModel> ProducerSplits,
    string RowVersion
);

/// <summary>
/// Read model for a commission line item.
/// </summary>
public sealed record LineItemReadModel(
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
/// Read model for a producer split.
/// </summary>
public sealed record ProducerSplitReadModel(
    Guid Id,
    Guid LineItemId,
    string ProducerName,
    Guid ProducerId,
    decimal SplitPercentage,
    decimal SplitAmount,
    string SplitAmountCurrency
);

/// <summary>
/// Filter for searching commission statements.
/// </summary>
public sealed class StatementSearchFilter
{
    /// <summary>
    /// Gets or sets the carrier ID filter.
    /// </summary>
    public Guid? CarrierId { get; set; }

    /// <summary>
    /// Gets or sets the status filter.
    /// </summary>
    public StatementStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the period month filter.
    /// </summary>
    public int? PeriodMonth { get; set; }

    /// <summary>
    /// Gets or sets the period year filter.
    /// </summary>
    public int? PeriodYear { get; set; }

    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Result of a statement search.
/// </summary>
public sealed class StatementSearchResult
{
    /// <summary>
    /// Gets or sets the statements.
    /// </summary>
    public IReadOnlyList<StatementListItemReadModel> Statements { get; set; } = [];

    /// <summary>
    /// Gets or sets the total count.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// Read model for a statement list item.
/// </summary>
public sealed record StatementListItemReadModel(
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
/// Commission statistics for the dashboard.
/// </summary>
public sealed record CommissionStatistics(
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
/// Entry for the commission summary report.
/// </summary>
public sealed record CommissionSummaryEntry(
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
/// Entry for the producer report.
/// </summary>
public sealed record ProducerReportEntry(
    Guid ProducerId,
    string ProducerName,
    int PeriodMonth,
    int PeriodYear,
    int LineItemCount,
    decimal TotalSplitAmount,
    decimal AverageSplitPercentage,
    string Currency
);
