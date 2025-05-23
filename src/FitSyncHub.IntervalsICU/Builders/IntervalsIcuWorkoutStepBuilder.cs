using System.Text;

namespace FitSyncHub.IntervalsICU.Builders;

public abstract class IntervalsIcuWorkoutStepBuilder
{
    protected readonly StringBuilder StringBuilder;

    protected IntervalsIcuWorkoutStepBuilder()
    {
        StringBuilder = new StringBuilder("- ");
    }

    public virtual string Build()
    {
        return StringBuilder.ToString();
    }

    protected void AppendTimeSegment(TimeSpan time)
    {
        if (time.Hours != 0)
        {
            StringBuilder.Append($"{time.Hours}h");
        }

        if (time.Minutes != 0)
        {
            StringBuilder.Append($"{time.Minutes}m");
        }

        if (time.Seconds != 0)
        {
            StringBuilder.Append($"{time.Seconds}s");
        }
    }
}
