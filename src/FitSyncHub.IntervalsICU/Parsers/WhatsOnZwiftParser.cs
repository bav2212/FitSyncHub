using System.Text;
using System.Text.RegularExpressions;

namespace FitSyncHub.IntervalsICU.Parsers;

public class WhatsOnZwiftParser
{
    private static readonly string s_timeHoursRegexKey = "hours";
    private static readonly string s_timeMinutesRegexKey = "minutes";
    private static readonly string s_timeSecondsRegexKey = "seconds";
    private static readonly string s_timeRegexPattern = $@"(?<time>((?<{s_timeHoursRegexKey}>\d+)hr\s*)?((?<{s_timeMinutesRegexKey}>\d+)min\s*)?((?<{s_timeSecondsRegexKey}>\d+)sec\s*)?)";

    private static readonly string s_repsNumberRegexKey = "reps_number";
    private static readonly string s_repsRegexPattern = $@"(?<reps>(?<{s_repsNumberRegexKey}>\d+)x\s*)?";

    private static readonly string s_rpmNumberRegexKey = "rpm_number";
    private static readonly string s_freeRideRegexKey = "free_ride";
    private static readonly string s_maxEffortRegexKey = "max_effort";
    private static readonly string s_ftpFromRegexKey = "ftp_from";
    private static readonly string s_ftpToRegexKey = "ftp_to";
    private static readonly string s_ftpSingleRegexKey = "ftp_single";

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
        stringBuilder.Append($@"(?<rpm>(?<{s_rpmNumberRegexKey}>\d+)rpm)?");
        // Non-greedy match for any characters in between
        stringBuilder.Append(@"(\s+?)?");
        // FTP segment
        stringBuilder.Append($@"(?<ftp>(from\s+(?<{s_ftpFromRegexKey}>\d+)\s+to\s+(?<{s_ftpToRegexKey}>\d+)%\s*FTP)|((?<{s_ftpSingleRegexKey}>\d+)% FTP))?");
        // free ride segment
        stringBuilder.Append($@"(?<{s_freeRideRegexKey}>(free ride)?)");
        // max effort segment
        stringBuilder.Append($@"(?<{s_maxEffortRegexKey}>(MAX)?)");

        // Regular expression to match the pattern
        s_pattern = stringBuilder.ToString();
    }

    public static IEnumerable<ParsedZwiftWorkoutGroup> Parse(IReadOnlyCollection<string> workoutSteps)
    {
        foreach (var (index, workoutStep) in workoutSteps.Select((x, i) => (i, x)))
        {
            var blockDescription = GetBlockDescription(workoutSteps, index);
            var lineSegments = workoutStep.Split(',', StringSplitOptions.TrimEntries);
            var workoutLines = ParseWorkoutLines(lineSegments).ToList();

            yield return new ParsedZwiftWorkoutGroup
            {
                BlockDescription = blockDescription,
                Items = workoutLines
            };
        }
    }

    private static IEnumerable<ParsedZwiftWorkoutLine> ParseWorkoutLines(string[] lineSegments)
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
            var isFreeRide = !string.IsNullOrEmpty(match.Groups[s_freeRideRegexKey].Value);
            var isMaxEffort = !string.IsNullOrEmpty(match.Groups[s_maxEffortRegexKey].Value);

            yield return new ParsedZwiftWorkoutLine
            {
                Time = time,
                Rpm = rpm,
                Ftp = ftp,
                IsFreeRide = isFreeRide,
                IsMaxEffort = isMaxEffort,
            };
        }
    }

    private static string GetBlockDescription(IReadOnlyCollection<string> lines, int index)
    {
        var repetitionRegexPattern = s_repsRegexPattern;

        // Match the line with the regular expression
        var repetitionMatch = Regex.Match(lines.ElementAt(index), repetitionRegexPattern);

        if (index == 0)
        {
            return "Warmup";
        }

        if (index == lines.Count - 1)
        {
            return "Cooldown";
        }

        // Get the repetition count if present (e.g., 3x)
        var repsNumberMatch = repetitionMatch.Groups[s_repsNumberRegexKey].Value.Trim();
        if (!string.IsNullOrEmpty(repsNumberMatch))
        {
            var repeatCount = int.Parse(repsNumberMatch);
            return $"{repeatCount}x";
        }

        return "1x";
    }

    private static IParsedZwiftWorkoutFtp? ConvertFtp(Match match)
    {
        var ftpFromMatch = match.Groups[s_ftpFromRegexKey].Value;
        var ftpToMatch = match.Groups[s_ftpToRegexKey].Value;
        // Check if it's an FTP range or a single value
        if (!string.IsNullOrEmpty(ftpFromMatch) && !string.IsNullOrEmpty(ftpToMatch))
        {
            return new ParsedZwiftWorkoutFtpRange
            {
                From = int.Parse(ftpFromMatch),
                To = int.Parse(ftpToMatch)
            };
        }

        var ftpSingleMatch = match.Groups[s_ftpSingleRegexKey].Value;
        if (!string.IsNullOrEmpty(ftpSingleMatch))
        {
            return new ParsedZwiftWorkoutFtpSingle
            {
                Value = int.Parse(ftpSingleMatch),
            };
        }

        return default;
    }

    private static int? ConvertRpm(Match match)
    {
        var rpm = match.Groups[s_rpmNumberRegexKey].Value;
        return string.IsNullOrEmpty(rpm) ? default(int?) : int.Parse(rpm);
    }

    private static TimeSpan ConvertTime(Match match)
    {
        var hoursMatch = match.Groups[s_timeHoursRegexKey].Value.Trim();
        var minutesMatch = match.Groups[s_timeMinutesRegexKey].Value.Trim();
        var secondsMatch = match.Groups[s_timeSecondsRegexKey].Value.Trim();

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
