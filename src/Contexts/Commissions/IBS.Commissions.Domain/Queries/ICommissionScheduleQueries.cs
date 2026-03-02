namespace IBS.Commissions.Domain.Queries;

/// <summary>
/// Read-side query interface for commission schedules.
/// </summary>
public interface ICommissionScheduleQueries
{
    /// <summary>
    /// Gets a schedule by its identifier for read operations.
    /// </summary>
    /// <param name="id">The schedule identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The schedule read model if found.</returns>
    Task<CommissionScheduleReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches schedules with filtering and pagination.
    /// </summary>
    /// <param name="filter">The search filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The search result with pagination.</returns>
    Task<ScheduleSearchResult> SearchAsync(ScheduleSearchFilter filter, CancellationToken cancellationToken = default);
}

/// <summary>
/// Read model for a commission schedule.
/// </summary>
public sealed record CommissionScheduleReadModel(
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
/// Filter for searching commission schedules.
/// </summary>
public sealed class ScheduleSearchFilter
{
    /// <summary>
    /// Gets or sets the carrier ID filter.
    /// </summary>
    public Guid? CarrierId { get; set; }

    /// <summary>
    /// Gets or sets the line of business filter.
    /// </summary>
    public string? LineOfBusiness { get; set; }

    /// <summary>
    /// Gets or sets the active status filter.
    /// </summary>
    public bool? IsActive { get; set; }

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
/// Result of a schedule search.
/// </summary>
public sealed class ScheduleSearchResult
{
    /// <summary>
    /// Gets or sets the schedules.
    /// </summary>
    public IReadOnlyList<CommissionScheduleReadModel> Schedules { get; set; } = [];

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
