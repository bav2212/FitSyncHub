namespace FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

public sealed record GarminTrainingPlanTaskItemResponse
{
    public required DateOnly CalendarDate { get; init; }
    public required GarminTrainingPlanTaskWorkoutResponse TaskWorkout { get; init; }
}
