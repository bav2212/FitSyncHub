using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using FitSyncHub.Common.Exceptions;

namespace FitSyncHub.Strava.Extensions;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> HandleJsonResponse<T>(
        this HttpResponseMessage response,
        JsonTypeInfo<T> jsonTypeInfo,
        CancellationToken cancellationToken)
    {
        try
        {
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken))!;
        }
        catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new NotFoundException("Entity not found", e);
        }
    }
}
