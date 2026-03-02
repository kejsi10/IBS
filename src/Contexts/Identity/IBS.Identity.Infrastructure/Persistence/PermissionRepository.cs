using IBS.Identity.Domain.Aggregates.Permission;
using IBS.Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.Identity.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Permission aggregate.
/// Following DDD patterns - only for persisting and retrieving aggregate roots.
/// </summary>
public sealed class PermissionRepository : IPermissionRepository
{
    private readonly DbContext _context;
    private readonly DbSet<Permission> _permissions;

    /// <summary>
    /// Initializes a new instance of the PermissionRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PermissionRepository(DbContext context)
    {
        _context = context;
        _permissions = context.Set<Permission>();
    }

    /// <inheritdoc />
    public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _permissions.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Permission aggregate, CancellationToken cancellationToken = default)
    {
        await _permissions.AddAsync(aggregate, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim().ToLowerInvariant();
        return await _permissions
            .FirstOrDefaultAsync(p => p.Name == normalizedName, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default)
    {
        return await _permissions
            .Where(p => p.Module == module)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _permissions
            .Where(p => idList.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }
}
