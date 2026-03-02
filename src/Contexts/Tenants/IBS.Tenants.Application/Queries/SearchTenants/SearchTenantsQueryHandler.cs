using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Tenants.Application.Queries.SearchTenants;

/// <summary>
/// Handler for SearchTenantsQuery.
/// </summary>
public sealed class SearchTenantsQueryHandler : IQueryHandler<SearchTenantsQuery, PagedResult<TenantListItemDto>>
{
    private readonly ITenantQueries _tenantQueries;

    /// <summary>
    /// Initializes a new instance of the SearchTenantsQueryHandler class.
    /// </summary>
    /// <param name="tenantQueries">The tenant queries.</param>
    public SearchTenantsQueryHandler(ITenantQueries tenantQueries)
    {
        _tenantQueries = tenantQueries;
    }

    /// <inheritdoc />
    public async Task<Result<PagedResult<TenantListItemDto>>> Handle(SearchTenantsQuery request, CancellationToken cancellationToken)
    {
        var result = await _tenantQueries.SearchAsync(
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result<PagedResult<TenantListItemDto>>.Success(result);
    }
}
