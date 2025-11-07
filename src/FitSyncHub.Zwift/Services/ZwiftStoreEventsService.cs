using System.Text.Json;
using System.Text.RegularExpressions;

namespace FitSyncHub.Zwift.HttpClients;

public partial class ZwiftStoreEventsService
{
    private readonly ZwiftHttpClient _zwiftHttpClient;

    public ZwiftStoreEventsService(ZwiftHttpClient zwiftHttpClient)
    {
        _zwiftHttpClient = zwiftHttpClient;
    }

    public async Task DownloadEventsAndStoreToFile(IReadOnlyCollection<string> zwiftEventURLs,
        string subgroupLabel,
        string storeToFolder,
        CancellationToken cancellationToken)
    {
        foreach (var zwiftEventUrl in zwiftEventURLs)
        {
            var zwiftEventId = GetEventId(zwiftEventUrl);

            var resultFilePath = Path.Combine(storeToFolder,
                subgroupLabel,
                zwiftEventId.ToString() + ".json");

            if (File.Exists(resultFilePath))
            {
                var jsonContent = await File.ReadAllTextAsync(resultFilePath, cancellationToken);
                if (IsRaceCompleted(jsonContent))
                {
                    continue;
                }
            }

            var zwiftEvent = await _zwiftHttpClient.GetEventFromZwfitEventViewUrl(zwiftEventUrl, cancellationToken);
            if (DateTime.UtcNow < zwiftEvent.EventStart)
            {
                continue;
            }

            if (DateTime.UtcNow < zwiftEvent.EventStart.AddHours(1.5))
            {
                // do not want to store unfinished activity
                continue;
            }

            var eventSubgroupId = zwiftEvent.EventSubgroups
                .Single(x => x.SubgroupLabel == subgroupLabel).Id;

            var content = await _zwiftHttpClient.GetEventSubgroupResults(eventSubgroupId, cancellationToken);

            if (JsonDocument.Parse(content)
                .RootElement.GetProperty("entries")
                .GetArrayLength() == 0)
            {
                continue;
            }

            File.WriteAllText(resultFilePath, content);
        }
    }

    private static int GetEventId(string zwiftEventUrl)
    {
        var regex = ZwiftEventIdRegex();
        var match = regex.Match(zwiftEventUrl);

        if (!match.Success || !int.TryParse(match.Groups["eventId"].Value, out var eventId))
        {
            throw new Exception("Wrong zwift event url format");
        }

        return eventId;
    }

    [GeneratedRegex("https:\\/\\/www\\.zwift\\.com\\/uk\\/events\\/view\\/(?<eventId>\\d+)(\\?)?")]
    private static partial Regex ZwiftEventIdRegex();

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
}
