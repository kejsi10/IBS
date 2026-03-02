using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Events;

namespace IBS.Identity.Domain.Aggregates.Role;

/// <summary>
/// Represents a role in the system.
/// </summary>
public sealed class Role : AggregateRoot
{
    private readonly List<RolePermission> _permissions = [];

    /// <summary>
    /// Gets the tenant identifier. Null for system-wide roles.
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Gets the role name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the normalized role name.
    /// </summary>
    public string NormalizedName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the role description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this is a system role.
    /// </summary>
    public bool IsSystemRole { get; private set; }

    /// <summary>
    /// Gets the permissions assigned to this role.
    /// </summary>
    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    /// <summary>
    /// Required by EF Core.
    /// </summary>
    private Role() { }

    /// <summary>
    /// Creates a new tenant-specific role.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="name">The role name.</param>
    /// <param name="description">The role description (optional).</param>
    /// <returns>A new Role instance.</returns>
    public static Role CreateTenantRole(Guid tenantId, string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));

        var role = new Role
        {
            TenantId = tenantId,
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            IsSystemRole = false
        };

        return role;
    }

    /// <summary>
    /// Creates a new system-wide role.
    /// </summary>
    /// <param name="name">The role name.</param>
    /// <param name="description">The role description (optional).</param>
    /// <returns>A new Role instance.</returns>
    public static Role CreateSystemRole(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));

        var role = new Role
        {
            TenantId = null,
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            Description = description?.Trim(),
            IsSystemRole = true
        };

        return role;
    }

    /// <summary>
    /// Updates the role information.
    /// </summary>
    /// <param name="name">The new role name.</param>
    /// <param name="description">The new description.</param>
    public void Update(string name, string? description)
    {
        if (IsSystemRole)
            throw new BusinessRuleViolationException("System roles cannot be modified.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required.", nameof(name));

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Description = description?.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Grants a permission to this role.
    /// </summary>
    /// <param name="permissionId">The permission identifier.</param>
    public void GrantPermission(Guid permissionId)
    {
        if (_permissions.Any(p => p.PermissionId == permissionId))
            return;

        var rolePermission = RolePermission.Create(Id, permissionId);
        _permissions.Add(rolePermission);
        MarkAsUpdated();

        if (TenantId.HasValue)
        {
            RaiseDomainEvent(new PermissionGrantedEvent(Id, TenantId.Value, permissionId));
        }
    }

    /// <summary>
    /// Revokes a permission from this role.
    /// </summary>
    /// <param name="permissionId">The permission identifier.</param>
    public void RevokePermission(Guid permissionId)
    {
        var rolePermission = _permissions.FirstOrDefault(p => p.PermissionId == permissionId);
        if (rolePermission == null)
            return;

        _permissions.Remove(rolePermission);
        MarkAsUpdated();
    }

    /// <summary>
    /// Checks if this role has a specific permission.
    /// </summary>
    /// <param name="permissionId">The permission identifier.</param>
    /// <returns>True if the role has the permission; otherwise, false.</returns>
    public bool HasPermission(Guid permissionId)
    {
        return _permissions.Any(p => p.PermissionId == permissionId);
    }
}
