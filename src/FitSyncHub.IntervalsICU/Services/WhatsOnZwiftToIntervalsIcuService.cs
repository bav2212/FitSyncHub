using System.Text.RegularExpressions;
using FitSyncHub.IntervalsICU.Builders;
using FitSyncHub.IntervalsICU.Models;
using FitSyncHub.IntervalsICU.Parsers;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.IntervalsICU.Services;

public partial class WhatsOnZwiftToIntervalsIcuService
{
    private readonly WhatsOnZwiftScraperService _whatsOnZwiftScraper;
    private readonly ILogger<WhatsOnZwiftToIntervalsIcuService> _logger;

    public WhatsOnZwiftToIntervalsIcuService(
        WhatsOnZwiftScraperService whatsOnZwiftScraper,
        ILogger<WhatsOnZwiftToIntervalsIcuService> logger)
    {
        _whatsOnZwiftScraper = whatsOnZwiftScraper;
        _logger = logger;
    }

    public async Task<WhatsOnZwiftToIntervalsIcuConvertResult> ScrapeAndConvertToIntervalsIcu(
        Uri workoutUri, CancellationToken cancellationToken)
    {
        var scrapeResult = await _whatsOnZwiftScraper.ScrapeWorkoutStructure(workoutUri, cancellationToken);

        var workout = WhatsOnZwiftParser.Parse(scrapeResult.WorkoutList);
        var workoutDescription = new IntervalsIcuWorkoutBuilder()
            .Build(workout);

        return new WhatsOnZwiftToIntervalsIcuConvertResult
        {
            IntervalsIcuWorkoutDescription = workoutDescription,
            FileInfo = GetFileNameInfo([.. scrapeResult.NameSegments])
        };
    }

    private WhatsOnZwiftToIntervalsIcuConvertFileInfo GetFileNameInfo(string[] nameSegments)
    {
        var (weekName, workoutName) = GetWeekAndWorkoutNameSegments(nameSegments);

        var week = GetWeek(weekName);
        TryGetDay(workoutName, out var day);

        var fileName = string.Join("-", nameSegments);

        return new WhatsOnZwiftToIntervalsIcuConvertFileInfo
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

    private static bool TryGetDay(string workoutName, out int dayNumber)
    {
        var dayMatch = WeekDayPattern().Match(workoutName);

        if (!dayMatch.Success)
        {
            dayNumber = default;
            return false;
        }

        return int.TryParse(dayMatch.Groups["day_number"].Value.Trim(), out dayNumber);
    }

    private WhatsOnZwiftToIntervalsIcuWeek GetWeek(string weekName)
    {
        var weekMatch = WeekPattern().Match(weekName);

        if (!weekMatch.Success)
        {
            _logger.LogWarning("Unexpected workout week segment");
            return WhatsOnZwiftToIntervalsIcuWeek.CreateWeek1();
        }

        var weekNumberMatch = weekMatch.Groups["week_number"].Value.Trim();
        if (!string.IsNullOrEmpty(weekNumberMatch))
        {
            return WhatsOnZwiftToIntervalsIcuWeek.Create(int.Parse(weekNumberMatch));
        }

        var weekZeroPreparationMatch = weekMatch.Groups["week_zero"].Value.Trim();
        if (!string.IsNullOrEmpty(weekZeroPreparationMatch))
        {
            return WhatsOnZwiftToIntervalsIcuWeek.CreatePreparationWeek();
        }

        throw new NotImplementedException();
    }

    [GeneratedRegex(@"^(Week\s(?<week_number>\d+))$|^(?<week_zero>Week 0 Prep)$")]
    private static partial Regex WeekPattern();
    [GeneratedRegex(@"^(Day\s(?<day_number>\d+))")]
    private static partial Regex WeekDayPattern();
}
