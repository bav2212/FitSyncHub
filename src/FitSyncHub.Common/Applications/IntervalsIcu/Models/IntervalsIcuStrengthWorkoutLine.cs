using System.Text;

namespace FitSyncHub.Common.Applications.IntervalsIcu.Models;

public record IntervalsIcuStrengthWorkoutLine : IIntervalsIcuWorkoutLine
{
    public required TimeSpan Time { get; init; }
    public required string ExerciseName { get; init; }

    public string ConvertToIntervalsIcuFormat()
    {
        var sb = new StringBuilder("-");
        sb.Append('"');
        sb.Append(ExerciseName);
        sb.Append('"');

        return sb.ToString();
    }
}
