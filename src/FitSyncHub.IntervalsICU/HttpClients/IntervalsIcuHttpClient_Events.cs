using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using ZwiftToIntervalsICUConverter.HttpClients.Models;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<EventResponse> GetEvent(
        string athleteId,
        int eventId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"api/v1/athlete/{athleteId}/events/{eventId}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSourceGenerationContext.Default.EventResponse)!;
    }

}
