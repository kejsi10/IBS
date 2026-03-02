namespace IBS.BuildingBlocks.Application;

/// <summary>
/// Abstraction for distributed caching operations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets or sets a cached value. If the key is not found, the factory function is called and the result is cached.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="factory">The factory function to produce the value if not cached.</param>
    /// <param name="ttl">The time-to-live for the cached value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached or newly computed value.</returns>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan ttl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached value by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cached values whose keys start with the given prefix.
    /// </summary>
    /// <param name="prefix">The key prefix.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
