using IBS.Carriers.Domain.ValueObjects;
using IBS.Policies.Domain.Aggregates.Policy;
using IBS.BuildingBlocks.Domain.ValueObjects;
using IBS.Policies.Domain.ValueObjects;

namespace IBS.Policies.Domain.Repositories;

/// <summary>
/// Repository interface for Policy aggregate operations.
/// </summary>
public interface IPolicyRepository
{
    /// <summary>
    /// Gets a policy by its identifier.
    /// </summary>
    /// <param name="id">The policy identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The policy if found; otherwise, null.</returns>
    Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a policy by its policy number.
    /// </summary>
    /// <param name="policyNumber">The policy number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The policy if found; otherwise, null.</returns>
    Task<Policy?> GetByPolicyNumberAsync(string policyNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all policies for a client.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of policies for the client.</returns>
    Task<IReadOnlyList<Policy>> GetByClientIdAsync(Guid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all policies for a carrier.
    /// </summary>
    /// <param name="carrierId">The carrier identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of policies for the carrier.</returns>
    Task<IReadOnlyList<Policy>> GetByCarrierIdAsync(Guid carrierId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets policies by status.
    /// </summary>
    /// <param name="status">The policy status.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of policies with the specified status.</returns>
    Task<IReadOnlyList<Policy>> GetByStatusAsync(PolicyStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets policies by line of business.
    /// </summary>
    /// <param name="lineOfBusiness">The line of business.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of policies for the line of business.</returns>
    Task<IReadOnlyList<Policy>> GetByLineOfBusinessAsync(LineOfBusiness lineOfBusiness, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active policies expiring within a date range.
    /// </summary>
    /// <param name="startDate">Start of the date range.</param>
    /// <param name="endDate">End of the date range.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of policies expiring within the range.</returns>
    Task<IReadOnlyList<Policy>> GetExpiringPoliciesAsync(DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets policies due for renewal (within specified days of expiration).
    /// </summary>
    /// <param name="daysUntilExpiration">Number of days until expiration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of policies due for renewal.</returns>
    Task<IReadOnlyList<Policy>> GetPoliciesDueForRenewalAsync(int daysUntilExpiration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches policies with filtering and pagination.
    /// </summary>
    /// <param name="filter">The search filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result of policies.</returns>
    Task<PolicySearchResult> SearchAsync(PolicySearchFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a policy number already exists.
    /// </summary>
    /// <param name="policyNumber">The policy number to check.</param>
    /// <param name="excludePolicyId">Optional policy ID to exclude from check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the policy number exists; otherwise, false.</returns>
    Task<bool> PolicyNumberExistsAsync(string policyNumber, Guid? excludePolicyId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new policy.
    /// </summary>
    /// <param name="policy">The policy to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AddAsync(Policy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing policy.
    /// </summary>
    /// <param name="policy">The policy to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateAsync(Policy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of policies by status for reporting.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of status to count.</returns>
    Task<Dictionary<PolicyStatus, int>> GetPolicyCountByStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total premium by line of business for reporting.
    /// </summary>
    /// <param name="effectiveYear">The effective year to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of line of business to total premium.</returns>
    Task<Dictionary<LineOfBusiness, decimal>> GetTotalPremiumByLineOfBusinessAsync(int effectiveYear, CancellationToken cancellationToken = default);
}

/// <summary>
/// Search filter for policies.
/// </summary>
public sealed class PolicySearchFilter
{
    /// <summary>
    /// Text search term (policy number, client name).
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by client ID.
    /// </summary>
    public Guid? ClientId { get; set; }

    /// <summary>
    /// Filter by carrier ID.
    /// </summary>
    public Guid? CarrierId { get; set; }

    /// <summary>
    /// Filter by policy status.
    /// </summary>
    public PolicyStatus? Status { get; set; }

    /// <summary>
    /// Filter by line of business.
    /// </summary>
    public LineOfBusiness? LineOfBusiness { get; set; }

    /// <summary>
    /// Filter by effective date from.
    /// </summary>
    public DateOnly? EffectiveDateFrom { get; set; }

    /// <summary>
    /// Filter by effective date to.
    /// </summary>
    public DateOnly? EffectiveDateTo { get; set; }

    /// <summary>
    /// Filter by expiration date from.
    /// </summary>
    public DateOnly? ExpirationDateFrom { get; set; }

    /// <summary>
    /// Filter by expiration date to.
    /// </summary>
    public DateOnly? ExpirationDateTo { get; set; }

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
/// Search result for policies.
/// </summary>
public sealed class PolicySearchResult
{
    /// <summary>
    /// The list of policies.
    /// </summary>
    public IReadOnlyList<Policy> Policies { get; set; } = [];

    /// <summary>
    /// Total count of matching policies.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
