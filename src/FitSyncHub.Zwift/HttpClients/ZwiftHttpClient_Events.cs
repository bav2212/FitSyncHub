using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.JsonSerializerContexts;

namespace FitSyncHub.Zwift.HttpClients;

public partial class ZwiftHttpClient
{
    public async Task<string> GetRaceResultsForSubgroup(int eventSubgroupId)
    {
        List<JsonElement> entriesResult = [];
        const long TakeCount = 50;

        do
        {
            var url = $"/api/race-results/entries?event_subgroup_id={eventSubgroupId}&limit={TakeCount}&start={entriesResult.Count}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            var entriesProperty = jsonDocument.RootElement.GetProperty("entries");
            var entriesDocuments = entriesProperty.EnumerateArray();
            entriesResult.AddRange(entriesDocuments);
        }
        while (entriesResult.Count > 0 && entriesResult.Count % TakeCount == 0);

        var mergedJson = new { entries = entriesResult }; // Create an object with "entries" key
        return JsonSerializer.Serialize(mergedJson, ZwiftEventsGenerationContext.Default.Options);
    }

    public async Task<ZwiftEventResponse> GetEvent(string zwiftEventUrl)
    {
        var url = zwiftEventUrl.Replace(
            "https://www.zwift.com/uk/events/view/",
            "/api/public/events/");

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize(content, ZwiftEventsGenerationContext.Default.ZwiftEventResponse)!;
    }
}
