using System.Globalization;
using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using ZwiftToIntervalsICUConverter.HttpClients.Models;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<IReadOnlyCollection<EventResponse>> ListEvents(
      string athleteId,
      DateTime oldest,
      DateTime newest,
      CancellationToken cancellationToken)
    {
        var baseUrl = $"api/v1/athlete/{athleteId}/events";

        var queryParams = new Dictionary<string, string>()
        {
            { "oldest", oldest.ToString("s", CultureInfo.InvariantCulture) },
            { "newest", newest.ToString("s", CultureInfo.InvariantCulture) },
        };

        var requestUri = $"{baseUrl}?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.IReadOnlyCollectionEventResponse)!;
    }

    public async Task<EventResponse> GetEvent(
        string athleteId,
        int eventId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"api/v1/athlete/{athleteId}/events/{eventId}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.EventResponse)!;
    }
}
