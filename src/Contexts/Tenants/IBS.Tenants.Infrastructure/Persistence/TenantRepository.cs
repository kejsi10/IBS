using IBS.Tenants.Domain.Aggregates.Tenant;
using IBS.Tenants.Domain.Repositories;
using IBS.Tenants.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IBS.Tenants.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Tenant aggregate.
/// Following DDD patterns - only for persisting and retrieving aggregate roots.
/// </summary>
public sealed class TenantRepository : ITenantRepository
{
    private readonly DbContext _context;
    private readonly DbSet<Tenant> _tenants;

    /// <summary>
    /// Initializes a new instance of the TenantRepository class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TenantRepository(DbContext context)
    {
        _context = context;
        _tenants = context.Set<Tenant>();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Uses FindAsync which checks tracked entities first before hitting the database.
    /// </remarks>
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _tenants.FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetByIdWithCarriersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _tenants
            .Include(t => t.Carriers)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant?> GetBySubdomainAsync(Subdomain subdomain, CancellationToken cancellationToken = default)
    {
        return await _tenants
            .Include(t => t.Carriers)
            .FirstOrDefaultAsync(t => t.Subdomain.Value == subdomain.Value, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(Tenant aggregate, CancellationToken cancellationToken = default)
    {
        await _tenants.AddAsync(aggregate, cancellationToken);
    }
}
