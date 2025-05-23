using FitSyncHub.Common.Workouts;
using FitSyncHub.IntervalsICU.Builders;

namespace FitSyncHub.IntervalsIcu.UnitTests;

public class IntervalsIcuWorkoutBuilderFtpUnitTest
{
    private readonly WorkoutType _workoutType = WorkoutType.Ride;
    private readonly TimeSpan _timespan = TimeSpan.FromMinutes(5);
    private readonly WorkoutStepType _workoutStepType = WorkoutStepType.Warmup;

    [Theory]
    [InlineData(25, 55, false, "Warmup\r\n- 5m 25-55%")]
    [InlineData(25, 55, true, "Warmup\r\n- 5m ramp 25-55%")]
    public void IntervalsIcuWorkoutBuilder_RangeFtp_WorkCorrectly(int fromFtp, int toFtp, bool isRamp, string expected)
    {
        var ftp = new RideFtpRange
        {
            From = fromFtp,
            To = toFtp,
            IsRampRange = isRamp
        };
        var workout = GetSUT(ftp);

        var actualWorkoutStructure = new IntervalsIcuWorkoutBuilder().Build(workout);
        Assert.Equal(expected, actualWorkoutStructure.Trim());
    }

    [Theory]
    [InlineData(25, "Warmup\r\n- 5m 25%")]
    [InlineData(55, "Warmup\r\n- 5m 55%")]
    public void IntervalsIcuWorkoutBuilder_SingleFtp_WorkCorrectly(int ftp, string expected)
    {
        var workout = GetSUT(new RideFtpSingle
        {
            Value = ftp,
        });

        var actualWorkoutStructure = new IntervalsIcuWorkoutBuilder().Build(workout);
        Assert.Equal(expected, actualWorkoutStructure.Trim());
    }

    private Workout GetSUT(IRideFtp ftp)
    {
        return new Workout
        {
            Type = _workoutType,
            Steps = [
                new RideWorkoutStep
                {
                    Type = _workoutStepType,
                    Time = _timespan,
                    Ftp = ftp,
                }
            ]
        };
    }
}
