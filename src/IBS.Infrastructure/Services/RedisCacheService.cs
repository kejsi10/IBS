using System.Text.Json;
using IBS.BuildingBlocks.Application;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace IBS.Infrastructure.Services;

/// <summary>
/// Redis-backed distributed cache service.
/// </summary>
public sealed class RedisCacheService(
    IDistributedCache cache,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc />
    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await cache.GetStringAsync(key, cancellationToken);
            if (cached is not null)
            {
                logger.LogDebug("Cache hit for key {CacheKey}", key);
                return JsonSerializer.Deserialize<T>(cached, JsonOptions)!;
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache read failed for key {CacheKey}, falling through to factory", key);
        }

        var value = await factory();

        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await cache.SetStringAsync(key, json, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            }, cancellationToken);
            logger.LogDebug("Cache set for key {CacheKey} with TTL {Ttl}", key, ttl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache write failed for key {CacheKey}", key);
        }

        return value;
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await cache.RemoveAsync(key, cancellationToken);
            logger.LogDebug("Cache removed key {CacheKey}", key);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache remove failed for key {CacheKey}", key);
        }
    }

    /// <inheritdoc />
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // Note: IDistributedCache does not support key scanning natively.
        // For a production system, use StackExchange.Redis IServer.Keys() directly.
        // For now, log a warning — individual key removal is preferred.
        logger.LogDebug(
            "RemoveByPrefixAsync called with prefix {Prefix}. " +
            "IDistributedCache does not support prefix deletion natively. " +
            "Use explicit key removal for known keys.",
            prefix);

        await Task.CompletedTask;
    }
}
