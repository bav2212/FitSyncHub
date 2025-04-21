using System.Text;
using FitSyncHub.Common.Applications.IntervalsIcu.Models;

namespace FitSyncHub.Common.Applications.IntervalsIcu;

public static class IntervalsIcuConverter
{
    // TODO. Return some strict model instead of string and this model should override ToString() and generate this string
    public static string ConvertToIntervalsIcuFormat(IReadOnlyCollection<IntervalsIcuWorkoutGroup> workoutGroups)
    {
        var mergedGroups = MergeGroups(workoutGroups).ToList();

        StringBuilder sb = new();

        foreach (var workoutGroup in mergedGroups)
        {
            sb.AppendLine(string.Empty);
            sb.AppendLine($"{workoutGroup.BlockInfo}");

            foreach (var workoutLine in workoutGroup.Items.Select(x => x.ConvertToIntervalsIcuFormat()))
            {
                sb.AppendLine(workoutLine);
            }
        }

        return sb.ToString();
    }

    private static IEnumerable<IntervalsIcuWorkoutGroup> MergeGroups(
        IReadOnlyCollection<IntervalsIcuWorkoutGroup> workoutGroups)
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
}
