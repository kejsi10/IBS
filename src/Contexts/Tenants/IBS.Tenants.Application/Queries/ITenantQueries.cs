namespace IBS.Tenants.Application.Queries;

/// <summary>
/// Read-only query interface for Tenant data.
/// All queries use no-tracking for optimal performance.
/// </summary>
public interface ITenantQueries
{
    /// <summary>
    /// Gets a tenant by identifier for display purposes.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant details if found; otherwise, null.</returns>
    Task<TenantDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tenants with pagination.
    /// </summary>
    /// <param name="searchTerm">Optional search term for name or subdomain.</param>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paginated list of tenants.</returns>
    Task<PagedResult<TenantListItemDto>> SearchAsync(
        string? searchTerm,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a subdomain is already in use.
    /// </summary>
    /// <param name="subdomain">The subdomain to check.</param>
    /// <param name="excludeTenantId">Tenant ID to exclude from check (for updates).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the subdomain is already in use.</returns>
    Task<bool> SubdomainExistsAsync(
        string subdomain,
        Guid? excludeTenantId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Tenant list item DTO.
/// </summary>
public sealed record TenantListItemDto
{
    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the tenant name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the subdomain.
    /// </summary>
    public string Subdomain { get; init; } = string.Empty;

    /// <summary>
    /// Gets the status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets the subscription tier.
    /// </summary>
    public string SubscriptionTier { get; init; } = string.Empty;

    /// <summary>
    /// Gets the creation date.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Tenant details DTO with full information.
/// </summary>
public sealed record TenantDetailsDto
{
    /// <summary>
    /// Gets the tenant identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the tenant name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the subdomain.
    /// </summary>
    public string Subdomain { get; init; } = string.Empty;

    /// <summary>
    /// Gets the status.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets the subscription tier.
    /// </summary>
    public string SubscriptionTier { get; init; } = string.Empty;

    /// <summary>
    /// Gets the default currency.
    /// </summary>
    public string DefaultCurrency { get; init; } = "USD";

    /// <summary>
    /// Gets the tenant carriers.
    /// </summary>
    public IReadOnlyList<TenantCarrierDto> Carriers { get; init; } = [];

    /// <summary>
    /// Gets the creation date.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the last update date.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Tenant carrier DTO.
/// </summary>
public sealed record TenantCarrierDto(
    Guid CarrierId,
    string? AgencyCode,
    decimal? CommissionRate,
    bool IsActive);

/// <summary>
/// Paginated result.
/// </summary>
/// <typeparam name="T">The type of items.</typeparam>
public sealed record PagedResult<T>
{
    /// <summary>
    /// Gets the items.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// Gets the current page.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Gets the page size.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total count.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Gets the total pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>
    /// Gets whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Gets whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
