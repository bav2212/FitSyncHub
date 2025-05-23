using FitSyncHub.Common.Workouts;
using FitSyncHub.IntervalsICU.Builders;

namespace FitSyncHub.IntervalsIcu.UnitTests;

public class IntervalsIcuWorkoutBuilderRideUnitTest
{
    [Fact]
    public void IntervalsIcuWorkoutBuilder_Builder_WorkCorrectly()
    {
        const string Expected =
            """
            Warmup
            - 5m 25-55%

            2x
            - 3m 100-110%
            - 4m 50-60%

            Cooldown
            - 5m 55-25%
            """;

        var steps = new List<WorkoutStep>
        {
            GetWarmupStep(),
            GetRepeatableStep(2, GetIntervalStep(), GetRecoveryStep()),
            GetCooldownStep()
        };

        var workout = new Workout { Type = WorkoutType.Ride, Steps = steps };

        var actualWorkoutStructure = new IntervalsIcuWorkoutBuilder().Build(workout);
        Assert.Equal(Expected, actualWorkoutStructure.Trim());
    }

    [Fact]
    public void IntervalsIcuWorkoutBuilder_BuilderWithSkipDoubledRecovery_WorkCorrectly()
    {
        const string Expected =
            """
            Warmup
            - 5m 25-55%

            - 3m 100-110%
            - 4m 50-60%
            - 3m 100-110%
            
            Cooldown
            - 5m 55-25%
            """;

        var steps = new List<WorkoutStep>
        {
            GetWarmupStep(),
            GetRepeatableStep(2, GetIntervalStep(), GetRecoveryStep()),
            GetCooldownStep()
        };

        var workout = new Workout { Type = WorkoutType.Ride, Steps = steps };

        var actualWorkoutStructure = new IntervalsIcuWorkoutBuilder()
            .WithSkipDoubledRecovery()
            .Build(workout);
        Assert.Equal(Expected, actualWorkoutStructure.Trim());
    }

    [Fact]
    public void IntervalsIcuWorkoutBuilder_BuilderWithSkipDoubledRecovery_InnerRepeatition_WorkCorrectly()
    {
        const string Expected =
            """
            Warmup
            - 5m 25-55%

            - 1m 100-110%
            - 2m 50-60%
            - 1m 100-110%
            - 2m 50-60%
            - 1m 100-110%
            - 5m 50-60%
            - 1m 100-110%
            - 2m 50-60%
            - 1m 100-110%
            - 2m 50-60%
            - 1m 100-110%

            Cooldown
            - 5m 55-25%
            """;

        var steps = new List<WorkoutStep>
        {
            GetWarmupStep(),
            GetRepeatableStep(
                2,
                GetRepeatableStep(3, GetIntervalStep(1), GetRecoveryStep(2)),
                GetRecoveryStep(5)),
            GetCooldownStep()
        };

        var workout = new Workout { Type = WorkoutType.Ride, Steps = steps };

        var actualWorkoutStructure = new IntervalsIcuWorkoutBuilder()
            .WithSkipDoubledRecovery()
            .Build(workout);
        Assert.Equal(Expected, actualWorkoutStructure.Trim());
    }

    private static RideWorkoutStep GetWarmupStep(long minutes = 5)
    {
        return new RideWorkoutStep
        {
            Type = WorkoutStepType.Warmup,
            Time = TimeSpan.FromMinutes(minutes),
            Ftp = new RideFtpRange { From = 25, To = 55, IsRampRange = false },
        };
    }

    private static RepeatableWorkoutStep GetRepeatableStep(int repeatiotion, params List<WorkoutStep> items)
    {
        return new RepeatableWorkoutStep
        {
            Type = WorkoutStepType.Interval,
            NumberOfIterations = repeatiotion,
            Items = items,
        };
    }

    private static RideWorkoutStep GetIntervalStep(long minutes = 3)
    {
        return new RideWorkoutStep
        {
            Type = WorkoutStepType.Interval,
            Time = TimeSpan.FromMinutes(minutes),
            Ftp = new RideFtpRange { From = 100, To = 110, IsRampRange = false },
        };
    }

    private static RideWorkoutStep GetRecoveryStep(long minutes = 4)
    {
        return new RideWorkoutStep
        {
            Type = WorkoutStepType.Recovery,
            Time = TimeSpan.FromMinutes(minutes),
            Ftp = new RideFtpRange { From = 50, To = 60, IsRampRange = false },
        };
    }

    private static RideWorkoutStep GetCooldownStep()
    {
        return new RideWorkoutStep
        {
            Type = WorkoutStepType.Cooldown,
            Time = TimeSpan.FromMinutes(5),
            Ftp = new RideFtpRange { From = 55, To = 25, IsRampRange = false },
        };
    }
}
