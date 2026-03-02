namespace IBS.Identity.Application.Queries;

/// <summary>
/// Read-only query interface for Role data.
/// All queries use no-tracking for optimal performance.
/// </summary>
public interface IRoleQueries
{
    /// <summary>
    /// Gets a role by identifier for display purposes.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The role details if found; otherwise, null.</returns>
    Task<RoleDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by name within a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier (null for system roles).</param>
    /// <param name="name">The role name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The role if found; otherwise, null.</returns>
    Task<RoleListItemDto?> GetByNameAsync(Guid? tenantId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles available to a tenant (tenant-specific + system roles).
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of roles available to the tenant.</returns>
    Task<IReadOnlyList<RoleListItemDto>> GetTenantRolesAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all system roles.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of system roles.</returns>
    Task<IReadOnlyList<RoleListItemDto>> GetSystemRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role name is already used within a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier (null for system roles).</param>
    /// <param name="name">The role name to check.</param>
    /// <param name="excludeRoleId">Role ID to exclude from check (for updates).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the name is already in use.</returns>
    Task<bool> NameExistsAsync(
        Guid? tenantId,
        string name,
        Guid? excludeRoleId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Role list item DTO.
/// </summary>
public sealed record RoleListItemDto
{
    /// <summary>
    /// Gets the role identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the tenant identifier (null for system roles).
    /// </summary>
    public Guid? TenantId { get; init; }

    /// <summary>
    /// Gets the role name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the role description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets whether this is a system role.
    /// </summary>
    public bool IsSystemRole { get; init; }

    /// <summary>
    /// Gets the number of users assigned to this role.
    /// </summary>
    public int UserCount { get; init; }
}

/// <summary>
/// Role details DTO with full information.
/// </summary>
public sealed record RoleDetailsDto
{
    /// <summary>
    /// Gets the role identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets the tenant identifier (null for system roles).
    /// </summary>
    public Guid? TenantId { get; init; }

    /// <summary>
    /// Gets the role name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the role description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets whether this is a system role.
    /// </summary>
    public bool IsSystemRole { get; init; }

    /// <summary>
    /// Gets the permissions assigned to this role.
    /// </summary>
    public IReadOnlyList<PermissionDto> Permissions { get; init; } = [];

    /// <summary>
    /// Gets the creation date.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Gets the last update date.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Permission DTO.
/// </summary>
public sealed record PermissionDto(Guid Id, string Name, string? Description, string Module);
