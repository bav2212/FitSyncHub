using System.Text.Json;
using System.Text.RegularExpressions;
using FitSyncHub.Zwift.HttpClients.Models.Responses;
using ZwiftToIntervalsICUConverter.HttpClients.Models;

namespace FitSyncHub.Zwift.HttpClients;

public partial class ZwiftDownloaderHttpClient
{
    private readonly HttpClient _httpClient;

    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public ZwiftDownloaderHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task FetchZwiftEventResultsAsync(IReadOnlyCollection<string> zwiftEventUrls,
        string subgroupLabel,
        string downloadToFolder)
    {
        foreach (var zwiftEventUrl in zwiftEventUrls)
        {
            var zwiftEventId = GetEventId(zwiftEventUrl);

            var resultFilePath = Path.Combine(downloadToFolder,
                subgroupLabel,
                zwiftEventId.ToString() + ".json");

            if (File.Exists(resultFilePath))
            {
                var jsonContent = await File.ReadAllTextAsync(resultFilePath);
                if (IsRaceCompleted(jsonContent))
                {
                    continue;
                }
            }

            var zwiftEventDto = await GetEventData(zwiftEventUrl);
            if (DateTime.UtcNow < zwiftEventDto.EventStart)
            {
                continue;
            }

            if (DateTime.UtcNow < zwiftEventDto.EventStart.AddHours(1.5))
            {
                // do not want to store unfinished activity
                continue;
            }

            var eventSubgroupId = zwiftEventDto.EventSubgroups
                .Single(x => x.SubgroupLabel == subgroupLabel).Id;

            var content = await GetRaceResultsForSubgroupAsync(eventSubgroupId);

            if (JsonDocument.Parse(content)
                .RootElement.GetProperty("entries")
                .GetArrayLength() == 0)
            {
                continue;
            }

            File.WriteAllText(resultFilePath, content);
        }
    }

    private static bool IsRaceCompleted(string jsonContent)
    {
        var jsonDocument = JsonDocument.Parse(jsonContent);
        var entriesProperty = jsonDocument.RootElement.GetProperty("entries");
        if (entriesProperty.GetArrayLength() == 0)
        {
            return false;
        }

        var lastEntity = entriesProperty.EnumerateArray().Last();
        var trainerDifficulty = lastEntity
            .GetProperty("sensorData")
            .GetProperty("trainerDifficulty")
            .GetDouble();
        return trainerDifficulty == 0 || trainerDifficulty == 1;
    }

    private static int GetEventId(string zwiftEventUrl)
    {
        var regex = ZwiftEventIdRegex();
        var match = regex.Match(zwiftEventUrl);

        if (!match.Success || !int.TryParse(match.Groups["eventId"].Value, out var eventId))
        {
            throw new Exception("Wrong url format");
        }

        return eventId;
    }

    private async Task<string> GetRaceResultsForSubgroupAsync(int eventSubgroupId)
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
        return JsonSerializer.Serialize(mergedJson, s_jsonOptions);
    }

    private async Task<ZwiftEventResponse> GetEventData(string zwiftEventUrl)
    {
        var url = zwiftEventUrl.Replace(
            "https://www.zwift.com/uk/events/view/",
            "/api/public/events/");

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize(content, ZwiftSourceGenerationContext.Default.ZwiftEventResponse)!;
    }

    [GeneratedRegex("https:\\/\\/www\\.zwift\\.com\\/uk\\/events\\/view\\/(?<eventId>\\d+)(\\?)?")]
    private static partial Regex ZwiftEventIdRegex();
}
