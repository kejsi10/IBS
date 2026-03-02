using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Aggregates.Permission;

namespace IBS.Identity.Domain.Repositories;

/// <summary>
/// Repository interface for Permission aggregate.
/// </summary>
public interface IPermissionRepository : IRepository<Permission>
{
    /// <summary>
    /// Gets a permission by name.
    /// </summary>
    /// <param name="name">The permission name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The permission if found; otherwise, null.</returns>
    Task<Permission?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a module.
    /// </summary>
    /// <param name="module">The module name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of permissions for the module.</returns>
    Task<IReadOnlyList<Permission>> GetByModuleAsync(string module, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets permissions by their identifiers.
    /// </summary>
    /// <param name="ids">The permission identifiers.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of permissions.</returns>
    Task<IReadOnlyList<Permission>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
}
