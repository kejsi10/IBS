using IBS.BuildingBlocks.Domain;
using IBS.Tenants.Domain.Aggregates.Tenant;
using IBS.Tenants.Domain.ValueObjects;

namespace IBS.Tenants.Domain.Repositories;

/// <summary>
/// Repository interface for Tenant aggregate.
/// Following DDD patterns - only for persisting and retrieving aggregate roots.
/// For read-only queries, use ITenantQueries.
/// </summary>
public interface ITenantRepository : IRepository<Tenant>
{
    /// <summary>
    /// Gets a tenant by identifier with all carriers loaded.
    /// Use this when you need to modify tenant carriers.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant with carriers if found; otherwise, null.</returns>
    Task<Tenant?> GetByIdWithCarriersAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by its subdomain.
    /// Used for tenant resolution - retrieves full aggregate for context setup.
    /// </summary>
    /// <param name="subdomain">The subdomain.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The tenant if found; otherwise, null.</returns>
    Task<Tenant?> GetBySubdomainAsync(Subdomain subdomain, CancellationToken cancellationToken = default);
}
