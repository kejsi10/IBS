using IBS.Identity.Application.Queries;
using IBS.Identity.Domain.Aggregates.Permission;
using Microsoft.EntityFrameworkCore;

namespace IBS.Identity.Infrastructure.Queries;

/// <summary>
/// Read-only query implementation for Permission data.
/// All queries use AsNoTracking for optimal performance.
/// </summary>
public sealed class PermissionQueries : IPermissionQueries
{
    private readonly DbSet<Permission> _permissions;

    /// <summary>
    /// Initializes a new instance of the PermissionQueries class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public PermissionQueries(DbContext context)
    {
        _permissions = context.Set<Permission>();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PermissionDto>> GetAllAsync(string? module = null, CancellationToken cancellationToken = default)
    {
        var query = _permissions.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(module))
        {
            query = query.Where(p => p.Module == module.Trim());
        }

        return await query
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Name)
            .Select(p => new PermissionDto(p.Id, p.Name, p.Description, p.Module))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PermissionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _permissions
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new PermissionDto(p.Id, p.Name, p.Description, p.Module))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
