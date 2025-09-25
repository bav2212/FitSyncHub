using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.JsonSerializerContexts;

namespace FitSyncHub.Zwift.HttpClients;

public partial class ZwiftHttpClient
{
    public async Task<ZwiftEventResponse> GetEvent(string eventUrl, CancellationToken cancellationToken)
    {
        var url = eventUrl.Replace(
            "https://www.zwift.com/uk/events/view/",
            "/api/public/events/");

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content, ZwiftEventsGenerationContext.Default.ZwiftEventResponse)!;
    }

    public async Task<string> GetEventSubgroupResults(int eventSubgroupId, CancellationToken cancellationToken)
    {
        List<JsonElement> entriesResult = [];
        const long TakeCount = 50;

        do
        {
            var url = $"/api/race-results/entries?event_subgroup_id={eventSubgroupId}&limit={TakeCount}&start={entriesResult.Count}";

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDocument = JsonDocument.Parse(content);
            var entriesProperty = jsonDocument.RootElement.GetProperty("entries");
            var entriesDocuments = entriesProperty.EnumerateArray();
            entriesResult.AddRange(entriesDocuments);
        }
        while (entriesResult.Count > 0 && entriesResult.Count % TakeCount == 0);

        var mergedJson = new { entries = entriesResult }; // Create an object with "entries" key
        return JsonSerializer.Serialize(mergedJson, ZwiftEventsGenerationContext.Default.Options);
    }

    public async Task<IReadOnlyCollection<ZwiftEventSubgroupEntrantResponse>> GetEventSubgroupEntrants(
        int eventSubgroupId,
        string type = "all",
        string participation = "signed_up",
        CancellationToken cancellationToken = default)
    {
        // see sauce source code how to handle pagination
        const long TakeCount = 100;
        const long Start = 0;

        var url = $"/api/events/subgroups/entrants/{eventSubgroupId}?type={type}&participation={participation}&limit={TakeCount}&start={Start}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
              ZwiftEventsGenerationContext.Default.IReadOnlyCollectionZwiftEventSubgroupEntrantResponse)!;
    }
}
