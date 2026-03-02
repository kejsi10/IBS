using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Claims.Domain.ValueObjects;

namespace IBS.Claims.Domain.Queries;

/// <summary>
/// Read-side query interface for claims.
/// </summary>
public interface IClaimQueries
{
    /// <summary>
    /// Gets a claim by its identifier for read operations.
    /// </summary>
    /// <param name="id">The claim identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The claim read model if found.</returns>
    Task<ClaimReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches claims with filtering and pagination.
    /// </summary>
    /// <param name="filter">The search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The search result with pagination.</returns>
    Task<ClaimSearchResult> SearchAsync(ClaimSearchFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets claim statistics for the dashboard.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Claim statistics.</returns>
    Task<ClaimStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Read model for a claim.
/// </summary>
public sealed record ClaimReadModel(
    Guid Id,
    string ClaimNumber,
    Guid PolicyId,
    Guid ClientId,
    string Status,
    DateTimeOffset LossDate,
    DateTimeOffset ReportedDate,
    string LossType,
    string LossDescription,
    decimal? LossAmount,
    string? LossAmountCurrency,
    decimal? ClaimAmount,
    string? ClaimAmountCurrency,
    string? AssignedAdjusterId,
    string? DenialReason,
    DateTimeOffset? ClosedAt,
    string? ClosureReason,
    Guid CreatedBy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ClaimNoteReadModel> Notes,
    IReadOnlyList<ReserveReadModel> Reserves,
    IReadOnlyList<ClaimPaymentReadModel> Payments,
    string RowVersion
);

/// <summary>
/// Read model for a claim note.
/// </summary>
public sealed record ClaimNoteReadModel(
    Guid Id,
    string Content,
    string AuthorBy,
    bool IsInternal,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Read model for a reserve.
/// </summary>
public sealed record ReserveReadModel(
    Guid Id,
    string ReserveType,
    decimal Amount,
    string Currency,
    string SetBy,
    DateTimeOffset SetAt,
    string? Notes
);

/// <summary>
/// Read model for a claim payment.
/// </summary>
public sealed record ClaimPaymentReadModel(
    Guid Id,
    string PaymentType,
    decimal Amount,
    string Currency,
    string PayeeName,
    DateOnly PaymentDate,
    string? CheckNumber,
    string Status,
    string AuthorizedBy,
    DateTimeOffset AuthorizedAt,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? VoidedAt,
    string? VoidReason
);

/// <summary>
/// Filter for searching claims.
/// </summary>
public sealed class ClaimSearchFilter
{
    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the status filter.
    /// </summary>
    public ClaimStatus? Status { get; set; }

    /// <summary>
    /// Gets or sets the policy ID filter.
    /// </summary>
    public Guid? PolicyId { get; set; }

    /// <summary>
    /// Gets or sets the client ID filter.
    /// </summary>
    public Guid? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the loss type filter.
    /// </summary>
    public LossType? LossType { get; set; }

    /// <summary>
    /// Gets or sets the loss date from filter.
    /// </summary>
    public DateTimeOffset? LossDateFrom { get; set; }

    /// <summary>
    /// Gets or sets the loss date to filter.
    /// </summary>
    public DateTimeOffset? LossDateTo { get; set; }

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
/// Result of a claims search.
/// </summary>
public sealed class ClaimSearchResult
{
    /// <summary>
    /// Gets or sets the claims.
    /// </summary>
    public IReadOnlyList<ClaimListItemReadModel> Claims { get; set; } = [];

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
/// Read model for a claim list item.
/// </summary>
public sealed record ClaimListItemReadModel(
    Guid Id,
    string ClaimNumber,
    Guid PolicyId,
    Guid ClientId,
    string Status,
    DateTimeOffset LossDate,
    DateTimeOffset ReportedDate,
    string LossType,
    decimal? LossAmount,
    string? LossAmountCurrency,
    decimal? ClaimAmount,
    string? ClaimAmountCurrency,
    string? AssignedAdjusterId,
    DateTimeOffset CreatedAt
);

/// <summary>
/// Statistics about claims for the dashboard.
/// </summary>
public sealed record ClaimStatistics(
    int TotalClaims,
    int OpenClaims,
    int ClosedClaims,
    int DeniedClaims,
    decimal TotalClaimAmount,
    decimal TotalPaidAmount,
    Dictionary<string, int> ClaimsByStatus,
    Dictionary<string, int> ClaimsByLossType
);
