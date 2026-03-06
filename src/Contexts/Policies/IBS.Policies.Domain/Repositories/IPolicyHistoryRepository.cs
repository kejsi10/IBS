using IBS.Policies.Domain.Aggregates.Policy;

namespace IBS.Policies.Domain.Repositories;

/// <summary>
/// Repository interface for PolicyHistory entries.
/// </summary>
public interface IPolicyHistoryRepository
{
    /// <summary>
    /// Adds a history entry.
    /// </summary>
    /// <param name="entry">The history entry to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(PolicyHistory entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated history entries for a policy, ordered by newest first.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="policyId">The policy identifier.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of history entries.</returns>
    Task<PolicyHistoryPage> GetByPolicyIdAsync(
        Guid tenantId,
        Guid policyId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Paginated result of policy history entries.
/// </summary>
public sealed class PolicyHistoryPage
{
    /// <summary>Gets the history entries for this page.</summary>
    public IReadOnlyList<PolicyHistory> Items { get; init; } = [];

    /// <summary>Gets the total number of history entries.</summary>
    public int TotalCount { get; init; }

    /// <summary>Gets the current page number.</summary>
    public int PageNumber { get; init; }

    /// <summary>Gets the page size.</summary>
    public int PageSize { get; init; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
