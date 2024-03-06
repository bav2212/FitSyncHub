using StravaWebhooksAzureFunctions.Exceptions;
using System.Net.Http.Json;
using System.Text.Json;

namespace StravaWebhooksAzureFunctions.Extensions;

public static class HttpResponseMessageExtensions
{
    public static Task<T> HandleJsonResponse<T>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        return HandleJsonResponse<T>(response, null!, cancellationToken);
    }

    public static async Task<T> HandleJsonResponse<T>(this HttpResponseMessage response, JsonSerializerOptions jsonSerializerOptions, CancellationToken cancellationToken = default)
    {
        try
        {
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<T>(jsonSerializerOptions, cancellationToken))!;
        }
        catch (HttpRequestException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new NotFoundException("Entity not found", e);
        }
        catch (HttpRequestException)
        {
            throw new HttpResponseMessageException("Error while http request executing", response);
        }
    }
}


