using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Tenants.Application.Queries.GetTenantById;

/// <summary>
/// Handler for GetTenantByIdQuery.
/// </summary>
public sealed class GetTenantByIdQueryHandler : IQueryHandler<GetTenantByIdQuery, TenantDetailsDto?>
{
    private readonly ITenantQueries _tenantQueries;

    /// <summary>
    /// Initializes a new instance of the GetTenantByIdQueryHandler class.
    /// </summary>
    /// <param name="tenantQueries">The tenant queries.</param>
    public GetTenantByIdQueryHandler(ITenantQueries tenantQueries)
    {
        _tenantQueries = tenantQueries;
    }

    /// <inheritdoc />
    public async Task<Result<TenantDetailsDto?>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _tenantQueries.GetByIdAsync(request.TenantId, cancellationToken);
        return Result<TenantDetailsDto?>.Success(tenant);
    }
}
