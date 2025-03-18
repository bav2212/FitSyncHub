using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses;
using ZwiftToIntervalsICUConverter.HttpClients.Models;

namespace FitSyncHub.Zwift.Helpers;

public static class ZwiftResultsAnalyzer
{
    public static async Task AnalyzeAsync(string directoryPath)
    {
        List<ZwiftRaceResultEntryResponse> results = [];
        foreach (var filePath in Directory.EnumerateFiles(directoryPath))
        {
            var fileContent = await File.ReadAllTextAsync(filePath);
            var parsedJson = JsonSerializer.Deserialize(fileContent, ZwiftSourceGenerationContext.Default.ZwiftRaceResultResponse)!;

            results.AddRange(parsedJson.Entries.Where(x => x.Qualified && !x.FlaggedCheating && !x.FlaggedSandbagging));
        }

        var orderedByDuration = results
            .OrderBy(x => x.ActivityData.DurationInMilliseconds)
            .ToList();

        var orderedByWattsPerKg = results
            .OrderByDescending(GetAvgWattsPerKg)
            .ToList();

        Console.WriteLine("Fastest:");
        FormatText(orderedByDuration, orderedByDuration.First());
        Console.WriteLine();

        Console.WriteLine("Strongest(watts per kg):");
        FormatText(orderedByDuration, orderedByWattsPerKg.First());
        Console.WriteLine();

        Console.WriteLine("Median by duration:");
        FormatText(orderedByDuration, orderedByDuration[orderedByDuration.Count / 2]);
        Console.WriteLine();

        Console.WriteLine("Median by watts per kg:");
        FormatText(orderedByDuration, orderedByWattsPerKg[orderedByWattsPerKg.Count / 2]);
        Console.WriteLine();

        Console.WriteLine("Average duration: " + Math.Round(orderedByDuration.Average(x => x.ActivityData.DurationInMilliseconds / 1000.0 / 60), 2));
        Console.WriteLine();

        var TUKRIds = await ZwiftPowerHelper.GetTeamUkraineRidersIds();

        var TUKRResults = orderedByDuration
            .Where(x => TUKRIds.Contains(x.ProfileId))
            .ToList();

        foreach (var TUKRResult in TUKRResults)
        {
            FormatText(orderedByDuration, TUKRResult);
            Console.WriteLine();
        }
    }

    private static void FormatText(
        List<ZwiftRaceResultEntryResponse> orderedByDurationItems,
        ZwiftRaceResultEntryResponse dto)
    {
        var name = $"{dto.ProfileData.FirstName} {dto.ProfileData.LastName}".Trim();
        Console.WriteLine(name + ":");

        var duration = dto.ActivityData.DurationInMilliseconds / 1000.0 / 60;
        var place = orderedByDurationItems.IndexOf(dto) + 1;

        Console.WriteLine("Duration: " + Math.Round(duration, 2)
            + ", Watts per kg: " + Math.Round(GetAvgWattsPerKg(dto), 2)
            + ", Place: " + place + "/" + orderedByDurationItems.Count);
    }

    private static double GetAvgWattsPerKg(ZwiftRaceResultEntryResponse dto)
    {
        var weight = dto.ProfileData.WeightInGrams / 1000.0;
        return dto.SensorData.AvgWatts / weight;
    }
}
