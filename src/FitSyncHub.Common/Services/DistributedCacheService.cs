using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Common.Services;

/// <summary>
/// extending IDistributedCache to add methods for serializing and deserializing objects
/// </summary>
public interface IDistributedCacheService : IDistributedCache
{
    Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken);
    Task SetValueAsync<T>(string key, T value, CancellationToken cancellationToken = default);
    Task SetValueAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default);
}

internal class DistributedCacheService : IDistributedCacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<DistributedCacheService> _logger;

    public DistributedCacheService(
        IDistributedCache distributedCache, ILogger<DistributedCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public byte[]? Get(string key) => _distributedCache.Get(key);
    public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => _distributedCache.GetAsync(key, token);
    public void Refresh(string key) => _distributedCache.Refresh(key);
    public Task RefreshAsync(string key, CancellationToken token = default) => _distributedCache.RefreshAsync(key, token);
    public void Remove(string key) => _distributedCache.Remove(key);
    public Task RemoveAsync(string key, CancellationToken token = default) => _distributedCache.RemoveAsync(key, token);
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _distributedCache.Set(key, value, options);
    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) => _distributedCache.SetAsync(key, value, options, token);

    public async Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken)
    {
        var serializedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (serializedValue == null)
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(serializedValue);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Cannot deserialize value: {Value}", serializedValue);
            await _distributedCache.RemoveAsync(key, cancellationToken);
            return default;
        }
    }

    public Task SetValueAsync<T>(
        string key, T value, CancellationToken cancellationToken = default)
    {
        return SetValueAsync(key, value, new DistributedCacheEntryOptions(), cancellationToken);
    }

    public async Task SetValueAsync<T>(
        string key, T value, DistributedCacheEntryOptions options, CancellationToken cancellationToken = default)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
    }
}
