namespace FitSyncHub.Common.Applications.IntervalsIcu.Models;

public sealed record IntervalsIcuWorkoutGroupBlockInfo
{
    private const int DefaultIntervalNumberOfIterations = 1;

    private IntervalsIcuWorkoutGroupBlockInfo()
    {
    }

    public int? NumberOfIterations { get; private init; }
    public IntervalsIcuWorkoutGroupType Type { get; private init; }
    public bool IsDefaultInterval => EqualsToDefaultInterval(this);

    public static IntervalsIcuWorkoutGroupBlockInfo CreateInterval(int numberOfIterations) => new()
    {
        Type = IntervalsIcuWorkoutGroupType.Interval,
        NumberOfIterations = numberOfIterations
    };

    public static IntervalsIcuWorkoutGroupBlockInfo CreateDefaultInterval()
        => CreateInterval(DefaultIntervalNumberOfIterations);

    public static IntervalsIcuWorkoutGroupBlockInfo CreateWarmup() => new()
    {
        Type = IntervalsIcuWorkoutGroupType.Warmup
    };
    public static IntervalsIcuWorkoutGroupBlockInfo CreateCooldown() => new()
    {
        Type = IntervalsIcuWorkoutGroupType.Cooldown
    };

    public static bool EqualsToDefaultInterval(IntervalsIcuWorkoutGroupBlockInfo item)
        => item.Type == IntervalsIcuWorkoutGroupType.Interval && item.NumberOfIterations == DefaultIntervalNumberOfIterations;

    public override string ToString()
    {
        return Type == IntervalsIcuWorkoutGroupType.Interval
            ? $"{NumberOfIterations}x"
            : Type.ToString();
    }
}
