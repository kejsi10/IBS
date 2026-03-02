using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Aggregates.Permission;

namespace IBS.Identity.Domain.Aggregates.Role;

/// <summary>
/// Represents the association between a role and a permission.
/// </summary>
public sealed class RolePermission : Entity
{
    /// <summary>
    /// Gets the role identifier.
    /// </summary>
    public Guid RoleId { get; private set; }

    /// <summary>
    /// Gets the permission identifier.
    /// </summary>
    public Guid PermissionId { get; private set; }

    /// <summary>
    /// Gets the permission (navigation property for queries).
    /// </summary>
    public Permission.Permission? Permission { get; private set; }

    /// <summary>
    /// Gets the date when this permission was granted.
    /// </summary>
    public DateTimeOffset GrantedAt { get; private set; }

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private RolePermission() { }

    /// <summary>
    /// Creates a new role-permission association.
    /// </summary>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="permissionId">The permission identifier.</param>
    /// <returns>A new RolePermission instance.</returns>
    internal static RolePermission Create(Guid roleId, Guid permissionId)
    {
        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId,
            GrantedAt = DateTimeOffset.UtcNow
        };
    }
}
