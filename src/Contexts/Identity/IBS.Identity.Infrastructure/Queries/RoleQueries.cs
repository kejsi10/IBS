using IBS.Identity.Application.Queries;
using IBS.Identity.Domain.Aggregates.Role;
using IBS.Identity.Domain.Aggregates.User;
using Microsoft.EntityFrameworkCore;

namespace IBS.Identity.Infrastructure.Queries;

/// <summary>
/// Read-only query implementation for Role data.
/// All queries use AsNoTracking for optimal performance.
/// </summary>
public sealed class RoleQueries : IRoleQueries
{
    private readonly DbContext _context;
    private readonly DbSet<Role> _roles;

    /// <summary>
    /// Initializes a new instance of the RoleQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public RoleQueries(DbContext context)
    {
        _context = context;
        _roles = context.Set<Role>();
    }

    /// <inheritdoc />
    public async Task<RoleDetailsDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roles
            .AsNoTracking()
            .Include(r => r.Permissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (role is null)
            return null;

        return new RoleDetailsDto
        {
            Id = role.Id,
            TenantId = role.TenantId,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            Permissions = role.Permissions
                .Where(rp => rp.Permission != null)
                .Select(rp => new PermissionDto(
                    rp.Permission!.Id,
                    rp.Permission.Name,
                    rp.Permission.Description,
                    rp.Permission.Module))
                .ToList(),
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }

    /// <inheritdoc />
    public async Task<RoleListItemDto?> GetByNameAsync(Guid? tenantId, string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToUpperInvariant();

        var role = await _roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.TenantId == tenantId &&
                r.NormalizedName == normalizedName,
                cancellationToken);

        if (role is null)
            return null;

        var userCount = await GetUserCountForRole(role.Id, cancellationToken);

        return new RoleListItemDto
        {
            Id = role.Id,
            TenantId = role.TenantId,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            UserCount = userCount
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RoleListItemDto>> GetTenantRolesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var roles = await _roles
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId || r.IsSystemRole)
            .OrderBy(r => r.IsSystemRole)
            .ThenBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var result = new List<RoleListItemDto>();
        foreach (var role in roles)
        {
            var userCount = await GetUserCountForRole(role.Id, cancellationToken);
            result.Add(new RoleListItemDto
            {
                Id = role.Id,
                TenantId = role.TenantId,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                UserCount = userCount
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RoleListItemDto>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roles
            .AsNoTracking()
            .Where(r => r.IsSystemRole)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var result = new List<RoleListItemDto>();
        foreach (var role in roles)
        {
            var userCount = await GetUserCountForRole(role.Id, cancellationToken);
            result.Add(new RoleListItemDto
            {
                Id = role.Id,
                TenantId = role.TenantId,
                Name = role.Name,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                UserCount = userCount
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<bool> NameExistsAsync(
        Guid? tenantId,
        string name,
        Guid? excludeRoleId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToUpperInvariant();

        var query = _roles
            .AsNoTracking()
            .Where(r => r.TenantId == tenantId && r.NormalizedName == normalizedName);

        if (excludeRoleId.HasValue)
        {
            query = query.Where(r => r.Id != excludeRoleId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    private async Task<int> GetUserCountForRole(Guid roleId, CancellationToken cancellationToken)
    {
        return await _context.Set<UserRole>()
            .AsNoTracking()
            .CountAsync(ur => ur.RoleId == roleId, cancellationToken);
    }
}
