using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.GetPermissions;

/// <summary>
/// Handler for the GetPermissionsQuery.
/// </summary>
public sealed class GetPermissionsQueryHandler(
    IPermissionQueries permissionQueries) : IQueryHandler<GetPermissionsQuery, IReadOnlyList<PermissionDto>>
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<PermissionDto>>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = await permissionQueries.GetAllAsync(request.Module, cancellationToken);
        return Result.Success<IReadOnlyList<PermissionDto>>(permissions);
    }
}
