using IBS.BuildingBlocks.Application;
using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.GetRoleById;

/// <summary>
/// Handler for the GetRoleByIdQuery.
/// </summary>
public sealed class GetRoleByIdQueryHandler(
    IRoleQueries roleQueries) : IQueryHandler<GetRoleByIdQuery, RoleDetailsDto>
{
    /// <inheritdoc />
    public async Task<Result<RoleDetailsDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await roleQueries.GetByIdAsync(request.RoleId, cancellationToken);

        if (role is null)
        {
            return Error.NotFound("Role", request.RoleId);
        }

        return role;
    }
}
