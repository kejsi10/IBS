using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.GetRoles;

/// <summary>
/// Handler for the GetRolesQuery.
/// </summary>
public sealed class GetRolesQueryHandler(
    IRoleQueries roleQueries) : IQueryHandler<GetRolesQuery, IReadOnlyList<RoleListItemDto>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<RoleListItemDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await roleQueries.GetTenantRolesAsync(request.TenantId, cancellationToken);
        return Result.Success<IReadOnlyList<RoleListItemDto>>(roles);
    }
}
