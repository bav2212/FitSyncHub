using FitSyncHub.Common.Extensions;
using FitSyncHub.Common.Workouts;
using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.Services;

public class GarminConnectToInternalWorkoutConverterService
{
    private readonly Func<WorkoutType, IGarminConnectToIntervalsIcuWorkoutStepConverterInitializer> _converterInitializerAccessor;

    public GarminConnectToInternalWorkoutConverterService(
        Func<WorkoutType, IGarminConnectToIntervalsIcuWorkoutStepConverterInitializer> converterInitializerAccessor)
    {
        _converterInitializerAccessor = converterInitializerAccessor;
    }

    public async Task<Workout> Convert(
        GarminWorkoutResponse workout,
        CancellationToken cancellationToken)
    {
        var workoutType = workout.SportType.SportTypeKey switch
        {
            "cycling" => WorkoutType.Ride,
            "strength_training" => WorkoutType.Strength,
            { } sportTypeKey => throw new NotImplementedException($"No converter found for sport type key: {sportTypeKey}")
        };

        var converter = await _converterInitializerAccessor(workoutType)
            .Initialize(cancellationToken);

        var workoutSteps = workout.WorkoutSegments[0].WorkoutSteps
            .Select(garminStep => Convert(converter, garminStep))
            .WhereNotNull()
            .ToList();

        return new Workout
        {
            Type = workoutType,
            Steps = workoutSteps
        };
    }

    private static WorkoutStep? Convert(
        IGarminConnectToIntervalsIcuWorkoutStepConverter converter,
        GarminWorkoutStepBase garminStep)
    {
        if (garminStep is GarminWorkoutExecutableStepResponse garminWorkoutStep)
        {
            return converter.Convert(garminWorkoutStep);
        }

        if (garminStep is GarminWorkoutRepeatGroupResponse garminRepeatStep)
        {
            return new RepeatableWorkoutStep
            {
                Type = WorkoutStepType.Interval,
                NumberOfIterations = garminRepeatStep.NumberOfIterations,
                Items = [.. garminRepeatStep.WorkoutSteps
                    .Select(x => Convert(converter, x))
                    .WhereNotNull()]
            };
        }

        throw new NotImplementedException();
    }
}
