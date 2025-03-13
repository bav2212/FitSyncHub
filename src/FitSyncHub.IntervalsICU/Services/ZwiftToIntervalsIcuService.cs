using System.Text.RegularExpressions;
using FitSyncHub.IntervalsICU.Parsers;
using FitSyncHub.IntervalsICU.Scrapers;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.IntervalsICU.Services;

public class ZwiftToIntervalsIcuService(ILogger<ZwiftToIntervalsIcuService> logger)
{
    public async Task<ZwiftToIntervalsIcuConvertResult> ScrapeAndConvertToIntervalsIcu(Uri workoutUri)
    {
        var scrapeResult = await WhatsOnZwiftScraper.ScrapeWorkoutStructure(workoutUri);

        var parsedRecords = WhatsOnZwiftParser.Parse(scrapeResult.WorkoutList).ToList();
        var resultLines = IntervalsIcuConverter.ConvertToIntervalsIcuFormat(parsedRecords);

        return new ZwiftToIntervalsIcuConvertResult
        {
            IntervalsIcuStructure = resultLines,
            FileInfo = GetFileNameInfo([.. scrapeResult.NameSegments])
        };
    }

    private ZwiftToIntervalsIcuConvertFileInfo GetFileNameInfo(string[] nameSegments)
    {
        var (weekName, workoutName) = GetWeekAndWorkoutNameSegments(nameSegments);

        var week = GetWeek(weekName);
        TryGetDay(workoutName, out var day);

        var fileName = string.Join("-", nameSegments);

        return new ZwiftToIntervalsIcuConvertFileInfo
        {
            Name = fileName,
            Week = week,
            Day = day
        };
    }

    private static (string weekName, string workoutName) GetWeekAndWorkoutNameSegments(string[] nameSegments)
    {
        if (nameSegments is [var first, var second])
        {
            return (first, second);
        }

        if (nameSegments is [var planName, var weekName, var workoutName])
        {
            _ = planName;
            return (weekName, workoutName);
        }

        throw new NotImplementedException();
    }

    private static bool TryGetDay(string workoutName, out int? dayNumber)
    {
        const string RegexPattern = @"^(Day\s(?<day_number>\d+))";
        // Match the line with the regular expression
        var dayMatch = Regex.Match(workoutName, RegexPattern);

        if (!dayMatch.Success)
        {
            dayNumber = default;
            return false;
        }

        dayNumber = int.Parse(dayMatch.Groups["day_number"].Value.Trim());
        return true;
    }

    private ZwiftToIntervalsIcuWeek GetWeek(string weekName)
    {
        const string RegexPattern = @"^(Week\s(?<week_number>\d+))$|^(?<week_zero>Week 0 Prep)$";
        // Match the line with the regular expression
        var weekMatch = Regex.Match(weekName, RegexPattern);

        if (!weekMatch.Success)
        {
            logger.LogWarning("Unexpected workout week segment");
            return ZwiftToIntervalsIcuWeek.CreateWeek1();
        }

        var weekNumberMatch = weekMatch.Groups["week_number"].Value.Trim();
        if (!string.IsNullOrEmpty(weekNumberMatch))
        {
            return ZwiftToIntervalsIcuWeek.Create(int.Parse(weekNumberMatch));
        }

        var weekZeroPreparationMatch = weekMatch.Groups["week_zero"].Value.Trim();
        if (!string.IsNullOrEmpty(weekZeroPreparationMatch))
        {
            return ZwiftToIntervalsIcuWeek.CreatePreparationWeek();
        }

        throw new NotImplementedException();
    }
}

public record ZwiftToIntervalsIcuConvertResult
{
    public required List<string> IntervalsIcuStructure { get; init; }
    public required ZwiftToIntervalsIcuConvertFileInfo FileInfo { get; init; }
}

public record ZwiftToIntervalsIcuConvertFileInfo
{
    public required string Name { get; init; }
    public required ZwiftToIntervalsIcuWeek Week { get; init; }
    public required int? Day { get; init; }
}

public record ZwiftToIntervalsIcuWeek
{
    public required bool IsPreparationWeek { get; init; }
    public required int WeekNumber { get; init; }

    public static ZwiftToIntervalsIcuWeek Create(int weekNumber)
    {
        return new ZwiftToIntervalsIcuWeek { WeekNumber = weekNumber, IsPreparationWeek = false };
    }

    public static ZwiftToIntervalsIcuWeek CreateWeek1()
    {
        return Create(1);
    }

    public static ZwiftToIntervalsIcuWeek CreatePreparationWeek()
    {
        return new ZwiftToIntervalsIcuWeek { WeekNumber = default, IsPreparationWeek = true };
    }
}
