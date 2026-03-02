using IBS.BuildingBlocks.Domain;
using IBS.Identity.Domain.Aggregates.User;

namespace IBS.Identity.Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate.
/// Following DDD patterns - only for persisting and retrieving aggregate roots for modification.
/// For read-only queries, use IUserQueries.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets a user by identifier with all related entities loaded.
    /// Use this when you need to modify user roles or refresh tokens.
    /// </summary>
    /// <param name="id">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user with related entities if found; otherwise, null.</returns>
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email address within a tenant.
    /// Used for authentication - retrieves full aggregate for password verification and login tracking.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="email">The email address.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by refresh token.
    /// Used for token refresh - retrieves full aggregate for token update.
    /// </summary>
    /// <param name="token">The refresh token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    Task<User?> GetByRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}
