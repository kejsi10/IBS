namespace IBS.Identity.Application.Queries;

/// <summary>
/// Read-only query interface for Permission data.
/// All queries use no-tracking for optimal performance.
/// </summary>
public interface IPermissionQueries
{
    /// <summary>
    /// Gets all permissions, optionally filtered by module.
    /// </summary>
    /// <param name="module">Optional module filter.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of permissions.</returns>
    Task<IReadOnlyList<PermissionDto>> GetAllAsync(string? module = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a permission by identifier.
    /// </summary>
    /// <param name="id">The permission identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The permission if found; otherwise, null.</returns>
    Task<PermissionDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
