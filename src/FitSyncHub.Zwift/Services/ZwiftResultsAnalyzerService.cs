using System.Text;
using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.Events;
using FitSyncHub.Zwift.JsonSerializerContexts;

namespace FitSyncHub.Zwift.Services;

public sealed class ZwiftResultsAnalyzerService
{
    private readonly ZwiftPowerService _zwiftPowerService;

    public ZwiftResultsAnalyzerService(ZwiftPowerService zwiftPowerService)
    {
        _zwiftPowerService = zwiftPowerService;
    }

    public async Task<string> AnalyzeAsync(string directoryPath)
    {
        List<ZwiftRaceResultEntryResponse> results = [];
        foreach (var filePath in Directory.EnumerateFiles(directoryPath))
        {
            var fileContent = await File.ReadAllTextAsync(filePath);
            var parsedJson = JsonSerializer.Deserialize(fileContent, ZwiftEventsGenerationContext.Default.ZwiftRaceResultResponse)!;

            results.AddRange(parsedJson.Entries.Where(x => x.Qualified && !x.FlaggedCheating && !x.FlaggedSandbagging));
        }

        var orderedByDuration = results
            .OrderBy(x => x.ActivityData.DurationInMilliseconds)
            .ToList();

        var orderedByWattsPerKg = results
            .OrderByDescending(GetAvgWattsPerKg)
            .ToList();

        var textBuilder = new ZwiftResultsTextBuilder();

        textBuilder.AppendLine("Fastest:");
        textBuilder.AppendFormattedText(orderedByDuration, 0);

        textBuilder.AppendLine("Strongest(watts per kg):");
        textBuilder.AppendFormattedText(orderedByWattsPerKg, 0);

        textBuilder.AppendLine("Median by duration:");
        textBuilder.AppendFormattedText(orderedByDuration, orderedByDuration.Count / 2);

        textBuilder.AppendLine("Median by watts per kg:");
        textBuilder.AppendFormattedText(orderedByWattsPerKg, orderedByWattsPerKg.Count / 2);

        textBuilder.AppendLine("Average duration: " + Math.Round(orderedByDuration.Average(x => x.ActivityData.DurationInMilliseconds / 1000.0 / 60), 2));
        textBuilder.AppendLine("");

#pragma warning disable IDE1006 // Naming Styles
        var TUKRIds = await _zwiftPowerService.GetTeamUkraineRidersIds();
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        var TUKRResults = orderedByDuration
            .Where(x => TUKRIds.Contains(x.ProfileId))
            .ToList();
#pragma warning restore IDE1006 // Naming Styles

#pragma warning disable IDE1006 // Naming Styles
        foreach (var TUKRResult in TUKRResults)
        {
            textBuilder.AppendFormattedText(orderedByDuration, TUKRResult);
        }
#pragma warning restore IDE1006 // Naming Styles

        return textBuilder.Build();
    }

    private static double GetAvgWattsPerKg(ZwiftRaceResultEntryResponse dto)
    {
        var weight = dto.ProfileData.WeightInGrams / 1000.0;
        return dto.SensorData.AvgWatts / weight;
    }

    private sealed class ZwiftResultsTextBuilder()
    {
        private readonly StringBuilder _sb = new();

        public void AppendLine(string line) => _sb.AppendLine(line);
        public string Build() => _sb.ToString();

        public void AppendFormattedText(
          List<ZwiftRaceResultEntryResponse> orderedByDurationItems,
          int dtoIndex)
        {
            var dto = orderedByDurationItems[dtoIndex];

            var name = $"{dto.ProfileData.FirstName} {dto.ProfileData.LastName}".Trim();
            _sb.AppendLine(name + ":");

            var duration = dto.ActivityData.DurationInMilliseconds / 1000.0 / 60;
            var place = dtoIndex + 1;

            _sb.AppendLine("Duration: " + Math.Round(duration, 2)
                + ", Watts per kg: " + Math.Round(GetAvgWattsPerKg(dto), 2)
                + ", Place: " + place + "/" + orderedByDurationItems.Count);

            _sb.AppendLine();
        }

        public void AppendFormattedText(
          List<ZwiftRaceResultEntryResponse> orderedByDurationItems,
          ZwiftRaceResultEntryResponse dto)
        {
            var index = orderedByDurationItems.IndexOf(dto);
            AppendFormattedText(orderedByDurationItems, index);
        }
    }
}
