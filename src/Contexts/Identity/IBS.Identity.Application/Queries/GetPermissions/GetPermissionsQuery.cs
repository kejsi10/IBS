using IBS.BuildingBlocks.Application.Queries;

namespace IBS.Identity.Application.Queries.GetPermissions;

/// <summary>
/// Query to get all permissions, optionally filtered by module.
/// </summary>
/// <param name="Module">Optional module filter.</param>
public sealed record GetPermissionsQuery(string? Module = null) : IQuery<IReadOnlyList<PermissionDto>>;
