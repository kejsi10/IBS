using IBS.BuildingBlocks.Domain;
using IBS.Carriers.Domain.ValueObjects;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Domain.Queries;

/// <summary>
/// Read-side query interface for quotes.
/// </summary>
public interface IQuoteQueries
{
    /// <summary>
    /// Gets a quote detail by its identifier.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="quoteId">The quote identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The quote detail if found; otherwise, null.</returns>
    Task<QuoteDetailReadModel?> GetByIdAsync(Guid tenantId, Guid quoteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches quotes with filtering and pagination.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="filter">The search filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated result of quotes.</returns>
    Task<QuoteSearchResult> SearchAsync(Guid tenantId, QuoteSearchFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quotes for a specific client.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated result of quotes.</returns>
    Task<QuoteSearchResult> GetByClientIdAsync(Guid tenantId, Guid clientId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets summary statistics for quotes.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Quote summary statistics.</returns>
    Task<QuoteSummaryStats> GetSummaryAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Read model for quote detail.
/// </summary>
public sealed record QuoteDetailReadModel(
    Guid Id,
    Guid ClientId,
    string? ClientName,
    string LineOfBusiness,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    string Status,
    DateOnly ExpiresAt,
    Guid? AcceptedCarrierId,
    Guid? PolicyId,
    string? Notes,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<QuoteCarrierReadModel> Carriers,
    string RowVersion) : IConcurrencyAware;

/// <summary>
/// Read model for quote carrier.
/// </summary>
public sealed record QuoteCarrierReadModel(
    Guid Id,
    Guid CarrierId,
    string? CarrierName,
    string Status,
    decimal? PremiumAmount,
    string? PremiumCurrency,
    string? DeclinationReason,
    string? Conditions,
    string? ProposedCoverages,
    DateTimeOffset? RespondedAt,
    DateOnly? ExpiresAt);

/// <summary>
/// Read model for quote list items.
/// </summary>
public sealed record QuoteListItemReadModel(
    Guid Id,
    Guid ClientId,
    string? ClientName,
    string LineOfBusiness,
    DateOnly EffectiveDate,
    DateOnly ExpirationDate,
    string Status,
    DateOnly ExpiresAt,
    int CarrierCount,
    int ResponseCount,
    decimal? LowestPremium,
    DateTimeOffset CreatedAt);

/// <summary>
/// Search filter for quotes.
/// </summary>
public sealed class QuoteSearchFilter
{
    /// <summary>
    /// Text search term.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by client ID.
    /// </summary>
    public Guid? ClientId { get; set; }

    /// <summary>
    /// Filter by status.
    /// </summary>
    public QuoteStatus? Status { get; set; }

    /// <summary>
    /// Filter by line of business.
    /// </summary>
    public LineOfBusiness? LineOfBusiness { get; set; }

    /// <summary>
    /// Page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size.
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort by field.
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// Sort direction.
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Search result for quotes.
/// </summary>
public sealed record QuoteSearchResult(
    IReadOnlyList<QuoteListItemReadModel> Quotes,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// Summary statistics for quotes dashboard.
/// </summary>
public sealed record QuoteSummaryStats(
    int TotalQuotes,
    int DraftCount,
    int SubmittedCount,
    int QuotedCount,
    int AcceptedCount,
    int ExpiredCount,
    int CancelledCount,
    decimal? AverageQuotedPremium);
