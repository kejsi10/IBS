using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Aggregates.Role;

namespace IBS.Identity.Domain.Repositories;

/// <summary>
/// Repository interface for Role aggregate.
/// Following DDD patterns - only for persisting and retrieving aggregate roots for modification.
/// For read-only queries, use IRoleQueries.
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Gets a role by identifier with permissions loaded.
    /// Use this when you need to modify role permissions.
    /// </summary>
    /// <param name="id">The role identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The role with permissions if found; otherwise, null.</returns>
    Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default);
}
