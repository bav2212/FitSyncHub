using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace FitSyncHub.Common.Extensions;
public static class DistributedCacheExtensions
{
    public static async Task<T?> GetFromJsonAsync<T>(
        this IDistributedCache cache, string key, CancellationToken cancellationToken)
    {
        var serializedValue = await cache.GetStringAsync(key, cancellationToken);
        if (serializedValue == null)
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(serializedValue);
        }
        catch (JsonException)
        {
            await cache.RemoveAsync(key, cancellationToken);
            return default;
        }
    }

    public static async Task SetAsJsonAsync<T>(
        this IDistributedCache cache, string key, T value, CancellationToken cancellationToken = default)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, serializedValue, token: cancellationToken);
    }
}
