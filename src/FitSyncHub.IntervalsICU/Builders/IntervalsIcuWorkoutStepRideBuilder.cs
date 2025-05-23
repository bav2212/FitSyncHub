using FitSyncHub.Common.Workouts;

namespace FitSyncHub.IntervalsICU.Builders;

public class IntervalsIcuWorkoutStepRideBuilder : IntervalsIcuWorkoutStepBuilder
{
    private readonly RideWorkoutStep _workoutStep;

    public IntervalsIcuWorkoutStepRideBuilder(RideWorkoutStep workoutStep)
    {
        _workoutStep = workoutStep;
    }

    public override string Build()
    {
        AppendTimeSegment(_workoutStep.Time);

        if (_workoutStep.IsFreeRide)
        {
            StringBuilder.Append(" freeride");
            return StringBuilder.ToString();
        }

        if (_workoutStep.IsMaxEffort)
        {
            // maxeffort doesn't visible on chart + not calculated in TSS, so adding Z6 as hack
            StringBuilder.Append(" maxeffort Z6");
            return StringBuilder.ToString();
        }

        if (_workoutStep.Rpm is { } rpm)
        {
            StringBuilder.Append($" {rpm}rpm");
        }

        AppendFtpSegment();

        return base.Build();
    }

    private void AppendFtpSegment()
    {
        if (_workoutStep.Ftp is null)
        {
            return;
        }

        StringBuilder.Append(' ');
        if (_workoutStep.Ftp is RideFtpRange ftpRange)
        {
            if (ftpRange.IsRampRange)
            {
                StringBuilder.Append("ramp ");
            }

            StringBuilder.Append($"{ftpRange.From}-{ftpRange.To}%");
            return;
        }

        if (_workoutStep.Ftp is RideFtpSingle ftpSingle)
        {
            StringBuilder.Append($"{ftpSingle.Value}%");
            return;
        }

        throw new NotImplementedException();
    }
}
