using FitSyncHub.Common.Applications.IntervalsIcu.Models;
using FitSyncHub.Common.Extensions;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Converters;

public static class GarminConnectToIntervalsIcuWorkoutConverter
{
    public static List<IntervalsIcuWorkoutGroup> Convert(
        WorkoutResponse workout,
        IGarminConnectToIntervalsIcuWorkoutStepConverter converter)
    {
        List<IntervalsIcuWorkoutGroup> result = [];

        foreach (var step in workout.WorkoutSegments[0].WorkoutSteps)
        {
            if (step is WorkoutExecutableStepResponse workoutStep)
            {
                var item = converter.Convert(workoutStep);

                var group = new IntervalsIcuWorkoutGroup
                {
                    BlockInfo = CreateBlockInfo(workoutStep),
                    Items = [item]
                };
                result.Add(group);
            }

            if (step is WorkoutRepeatGroupResponse repeatStep)
            {
                var items = GarminConnectWorkoutHelper.GetFlattenWorkoutSteps(repeatStep)
                    .Select(converter.Convert)
                    .WhereNotNull()
                    .ToList();

                var group = new IntervalsIcuWorkoutGroup
                {
                    BlockInfo = IntervalsIcuWorkoutGroupBlockInfo.CreateInterval(repeatStep.NumberOfIterations),
                    Items = items
                };
                result.Add(group);
            }
        }

        return result;
    }

    private static IntervalsIcuWorkoutGroupBlockInfo CreateBlockInfo(
        WorkoutExecutableStepResponse workoutStep)
    {
        return GarminConnectWorkoutHelper.GetWorkoutGroupType(workoutStep) switch
        {
            IntervalsIcuWorkoutGroupType.Warmup => IntervalsIcuWorkoutGroupBlockInfo.CreateWarmup(),
            IntervalsIcuWorkoutGroupType.Cooldown => IntervalsIcuWorkoutGroupBlockInfo.CreateCooldown(),
            IntervalsIcuWorkoutGroupType.Interval => IntervalsIcuWorkoutGroupBlockInfo.CreateDefaultInterval(),
            _ => throw new NotImplementedException(),
        };
    }
}
