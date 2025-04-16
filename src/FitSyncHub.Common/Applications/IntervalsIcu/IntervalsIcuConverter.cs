using System.Text;
using FitSyncHub.Common.Applications.IntervalsIcu.Models;

namespace FitSyncHub.Common.Applications.IntervalsIcu;

public static class IntervalsIcuConverter
{
    // to do. Return some strict model instead of string and this model should override ToString() and generate this string
    public static string ConvertToIntervalsIcuFormat(IReadOnlyCollection<IntervalsIcuWorkoutGroup> workoutGroups)
    {
        var mergedGroups = MergeGroups(workoutGroups).ToList();

        StringBuilder sb = new();

        foreach (var workoutGroup in mergedGroups)
        {
            sb.AppendLine(string.Empty);
            sb.AppendLine($"{workoutGroup.BlockInfo}");

            foreach (var workoutLine in workoutGroup.Items.Select(ConvertToIntervalsIcuFormat))
            {
                sb.AppendLine(workoutLine);
            }
        }

        return sb.ToString();
    }

    private static IEnumerable<IntervalsIcuWorkoutGroup> MergeGroups(IReadOnlyCollection<IntervalsIcuWorkoutGroup> workoutGroups)
    {
        if (workoutGroups.Count == 0)
        {
            yield break;
        }

        IntervalsIcuWorkoutGroup? previousGroup = null;

        foreach (var workoutGroup in workoutGroups)
        {
            if (previousGroup is null)
            {
                previousGroup = workoutGroup;
                continue;
            }

            if (previousGroup.BlockInfo == workoutGroup.BlockInfo && workoutGroup.BlockInfo.IsDefaultInterval)
            {
                previousGroup = previousGroup with
                {
                    Items = [.. previousGroup.Items, .. workoutGroup.Items]
                };
                continue;
            }

            yield return previousGroup;
            previousGroup = workoutGroup;
        }

        yield return previousGroup!;
    }

    private static string ConvertToIntervalsIcuFormat(IntervalsIcuWorkoutLine item)
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

    private static void AppendTimeSegment(IntervalsIcuWorkoutLine item, StringBuilder sb)
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

    private static void AppendFtpSegment(IntervalsIcuWorkoutLine item, StringBuilder sb)
    {
        if (item.Ftp is null)
        {
            return;
        }

        sb.Append(' ');
        if (item.Ftp is IntervalsIcuWorkoutFtpRange ftpRange)
        {
            if (ftpRange.IsRampRange)
            {
                sb.Append("ramp ");
            }

            sb.Append($"{ftpRange.From}-{ftpRange.To}%");
            return;
        }

        if (item.Ftp is IntervalsIcuWorkoutFtpSingle ftpSingle)
        {
            sb.Append($"{ftpSingle.Value}%");
            return;
        }

        throw new NotImplementedException();
    }
}
