using System.Text;
using System.Text.RegularExpressions;
using FitSyncHub.Common.Applications.IntervalsIcu.Models;

namespace FitSyncHub.IntervalsICU.Parsers;

public class WhatsOnZwiftParser
{
    private const string TimeHoursRegexKey = "hours";
    private const string TimeMinutesRegexKey = "minutes";
    private const string TimeSecondsRegexKey = "seconds";
    private static readonly string s_timeRegexPattern = $@"(?<time>((?<{TimeHoursRegexKey}>\d+)hr\s*)?((?<{TimeMinutesRegexKey}>\d+)min\s*)?((?<{TimeSecondsRegexKey}>\d+)sec\s*)?)";

    private const string RepsNumberRegexKey = "reps_number";
    private static readonly string s_repsRegexPattern = $@"(?<reps>(?<{RepsNumberRegexKey}>\d+)x\s*)?";

    private const string RpmNumberRegexKey = "rpm_number";
    private const string FreeRideRegexKey = "free_ride";
    private const string MaxEffortRegexKey = "max_effort";
    private const string FtpFromRegexKey = "ftp_from";
    private const string FtpToRegexKey = "ftp_to";
    private const string FtpSingleRegexKey = "ftp_single";

    private static readonly string s_pattern;

    static WhatsOnZwiftParser()
    {
        StringBuilder stringBuilder = new();
        // capture repetition
        stringBuilder.Append(s_repsRegexPattern);
        // Time segment
        stringBuilder.Append(s_timeRegexPattern);
        stringBuilder.Append(@"(\s*@\s*)?");
        // Optional RPM segment
        stringBuilder.Append($@"(?<rpm>(?<{RpmNumberRegexKey}>\d+)rpm)?");
        // Non-greedy match for any characters in between
        stringBuilder.Append(@"(\s+?)?");
        // FTP segment
        stringBuilder.Append($@"(?<ftp>(from\s+(?<{FtpFromRegexKey}>\d+)\s+to\s+(?<{FtpToRegexKey}>\d+)%\s*FTP)|((?<{FtpSingleRegexKey}>\d+)% FTP))?");
        // free ride segment
        stringBuilder.Append($"(?<{FreeRideRegexKey}>(free ride)?)");
        // max effort segment
        stringBuilder.Append($"(?<{MaxEffortRegexKey}>(MAX)?)");

        // Regular expression to match the pattern
        s_pattern = stringBuilder.ToString();
    }

    public static IEnumerable<IntervalsIcuWorkoutGroup> Parse(IReadOnlyCollection<string> workoutSteps)
    {
        foreach (var (index, workoutStep) in workoutSteps.Index())
        {
            var blockInfo = GetBlockInfo(workoutSteps, index);
            var lineSegments = workoutStep.Split(',', StringSplitOptions.TrimEntries);
            var workoutLines = ParseWorkoutLines(lineSegments).ToList();

            yield return new IntervalsIcuWorkoutGroup
            {
                BlockInfo = blockInfo,
                Items = workoutLines
            };
        }
    }

    private static IEnumerable<IntervalsIcuWorkoutLine> ParseWorkoutLines(string[] lineSegments)
    {
        foreach (var line in lineSegments)
        {
            // Match the line with the regular expression
            var match = Regex.Match(line, s_pattern);

            if (!match.Success)
            {
                throw new Exception("Line format not recognized: " + line);
            }

            var time = ConvertTime(match);
            var rpm = ConvertRpm(match);
            var ftp = ConvertFtp(match);
            var isFreeRide = !string.IsNullOrEmpty(match.Groups[FreeRideRegexKey].Value);
            var isMaxEffort = !string.IsNullOrEmpty(match.Groups[MaxEffortRegexKey].Value);

            yield return new IntervalsIcuWorkoutLine
            {
                Time = time,
                Rpm = rpm,
                Ftp = ftp,
                IsFreeRide = isFreeRide,
                IsMaxEffort = isMaxEffort,
            };
        }
    }

    private static IntervalsIcuWorkoutGroupBlockInfo GetBlockInfo(IReadOnlyCollection<string> lines, int index)
    {
        var repetitionRegexPattern = s_repsRegexPattern;

        // Match the line with the regular expression
        var repetitionMatch = Regex.Match(lines.ElementAt(index), repetitionRegexPattern);

        if (index == 0)
        {
            return IntervalsIcuWorkoutGroupBlockInfo.CreateWarmup();
        }

        if (index == lines.Count - 1)
        {
            return IntervalsIcuWorkoutGroupBlockInfo.CreateCooldown();
        }

        // Get the repetition count if present (e.g., 3x)
        var repsNumberMatch = repetitionMatch.Groups[RepsNumberRegexKey].Value.Trim();

        var repeatCount = !string.IsNullOrEmpty(repsNumberMatch)
            && int.TryParse(repsNumberMatch, out var parsedRepeatCount)
            ? parsedRepeatCount
            : 1;

        return IntervalsIcuWorkoutGroupBlockInfo.CreateInterval(repeatCount);
    }

    private static IIntervalsIcuWorkoutFtp? ConvertFtp(Match match)
    {
        var ftpFromMatch = match.Groups[FtpFromRegexKey].Value;
        var ftpToMatch = match.Groups[FtpToRegexKey].Value;
        // Check if it's an FTP range or a single value
        if (!string.IsNullOrEmpty(ftpFromMatch) && !string.IsNullOrEmpty(ftpToMatch))
        {
            return new IntervalsIcuWorkoutFtpRange
            {
                From = int.Parse(ftpFromMatch),
                To = int.Parse(ftpToMatch),
                IsRampRange = true,
            };
        }

        var ftpSingleMatch = match.Groups[FtpSingleRegexKey].Value;
        if (!string.IsNullOrEmpty(ftpSingleMatch))
        {
            return new IntervalsIcuWorkoutFtpSingle
            {
                Value = int.Parse(ftpSingleMatch),
            };
        }

        return default;
    }

    private static int? ConvertRpm(Match match)
    {
        var rpm = match.Groups[RpmNumberRegexKey].Value;
        return string.IsNullOrEmpty(rpm) ? default(int?) : int.Parse(rpm);
    }

    private static TimeSpan ConvertTime(Match match)
    {
        var hoursMatch = match.Groups[TimeHoursRegexKey].Value.Trim();
        var minutesMatch = match.Groups[TimeMinutesRegexKey].Value.Trim();
        var secondsMatch = match.Groups[TimeSecondsRegexKey].Value.Trim();

        var hours = string.IsNullOrEmpty(hoursMatch) ? default : int.Parse(hoursMatch);
        var minutes = string.IsNullOrEmpty(minutesMatch) ? default : int.Parse(minutesMatch);

        try
        {
            var seconds = string.IsNullOrEmpty(secondsMatch) ? default : int.Parse(secondsMatch);
            return new TimeSpan(hours, minutes, seconds);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
