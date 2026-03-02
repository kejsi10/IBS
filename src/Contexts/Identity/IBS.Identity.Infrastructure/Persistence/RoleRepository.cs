using IBS.Identity.Domain.Aggregates.Role;
using IBS.Identity.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IBS.Identity.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Role aggregate.
/// Following DDD patterns - only for persisting and retrieving aggregate roots.
/// </summary>
public sealed class RoleRepository : IRoleRepository
{
    private readonly DbContext _context;
    private readonly DbSet<Role> _roles;

    /// <summary>
    /// Initializes a new instance of the RoleRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public RoleRepository(DbContext context)
    {
        _context = context;
        _roles = context.Set<Role>();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Uses FindAsync which checks tracked entities first before hitting the database.
    /// </remarks>
    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _roles.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _roles
            .Include(r => r.Permissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Role aggregate, CancellationToken cancellationToken = default)
    {
        await _roles.AddAsync(aggregate, cancellationToken);
    }
}
