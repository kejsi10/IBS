using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.GetRoleById;

/// <summary>
/// Query to get a role by its identifier.
/// </summary>
/// <param name="RoleId">The role identifier.</param>
public sealed record GetRoleByIdQuery(Guid RoleId) : IQuery<RoleDetailsDto>;
