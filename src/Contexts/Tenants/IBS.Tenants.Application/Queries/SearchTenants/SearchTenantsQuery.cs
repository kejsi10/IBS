using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Tenants.Application.Queries.SearchTenants;

/// <summary>
/// Query to search tenants.
/// </summary>
/// <param name="SearchTerm">Optional search term for name or subdomain.</param>
/// <param name="Page">The page number (1-based).</param>
/// <param name="PageSize">The page size.</param>
public sealed record SearchTenantsQuery(
    string? SearchTerm,
    int Page,
    int PageSize) : IQuery<PagedResult<TenantListItemDto>>;
