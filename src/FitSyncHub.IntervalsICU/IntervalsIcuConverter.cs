using System.Text;
using FitSyncHub.IntervalsICU.Parsers;

namespace FitSyncHub.IntervalsICU;

public class IntervalsIcuConverter
{
    public static List<string> ConvertToIntervalsIcuFormat(IReadOnlyCollection<ParsedZwiftWorkoutGroup> workoutGroups)
    {
        var mergedGroups = MergeGroups(workoutGroups).ToList();

        List<string> resultLines = [];

        foreach (var workoutGroup in mergedGroups)
        {
            resultLines.Add(string.Empty);
            resultLines.Add($"{workoutGroup.BlockDescription}");

            resultLines.AddRange(workoutGroup.Items.Select(ConvertToIntervalsIcuFormat));
        }

        return resultLines;
    }

    private static IEnumerable<ParsedZwiftWorkoutGroup> MergeGroups(IReadOnlyCollection<ParsedZwiftWorkoutGroup> workoutGroups)
    {
        if (workoutGroups.Count == 0)
        {
            yield break;
        }

        ParsedZwiftWorkoutGroup? previousGroup = null;

        foreach (var workoutGroup in workoutGroups)
        {
            if (previousGroup is null)
            {
                previousGroup = workoutGroup;
                continue;
            }

            if (previousGroup.BlockDescription == workoutGroup.BlockDescription
                && workoutGroup.BlockDescription == "1x")
            {
                previousGroup = previousGroup with
                {
                    Items = [.. previousGroup.Items, .. workoutGroup.Items]
                };
            }
            else
            {
                yield return previousGroup;
                previousGroup = workoutGroup;
                continue;
            }
        }

        yield return previousGroup!;
    }

    private static string ConvertToIntervalsIcuFormat(ParsedZwiftWorkoutLine item)
    {
        var sb = new StringBuilder("-");
        AppendTimeSegment(item, sb);

        if (item.IsFreeRide)
        {
            sb.Append(" freeride");
            return sb.ToString();
        }

        if (item.IsMaxEffort)
        {
            // maxeffort doesn't visible on chart + not calculated in TSS, so adding Z6 as hack
            sb.Append(" maxeffort Z6");
            return sb.ToString();
        }

        if (item.Rpm is { } rpm)
        {
            sb.Append($" {rpm}rpm");
        }

        AppendFtpSegment(item, sb);
        return sb.ToString();
    }

    private static void AppendTimeSegment(ParsedZwiftWorkoutLine item, StringBuilder sb)
    {
        sb.Append(' ');
        if (item.Time.Hours != 0)
        {
            sb.Append($"{item.Time.Hours}h");
        }

        if (item.Time.Minutes != 0)
        {
            sb.Append($"{item.Time.Minutes}m");
        }

        if (item.Time.Seconds != 0)
        {
            sb.Append($"{item.Time.Seconds}s");
        }
    }

    private static void AppendFtpSegment(ParsedZwiftWorkoutLine item, StringBuilder sb)
    {
        if (item.Ftp is null)
        {
            return;
        }

        sb.Append(' ');
        if (item.Ftp is ParsedZwiftWorkoutFtpRange ftpRange)
        {
            sb.Append($"ramp {ftpRange.From}-{ftpRange.To}%");
            return;
        }

        if (item.Ftp is ParsedZwiftWorkoutFtpSingle ftpSingle)
        {
            sb.Append($"{ftpSingle.Value}%");
            return;
        }

        throw new NotImplementedException();
    }
}
