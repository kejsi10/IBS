using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.GetRoles;

/// <summary>
/// Query to get all roles available to a tenant.
/// </summary>
/// <param name="TenantId">The tenant identifier.</param>
public sealed record GetRolesQuery(Guid TenantId) : IQuery<IReadOnlyList<RoleListItemDto>>;
