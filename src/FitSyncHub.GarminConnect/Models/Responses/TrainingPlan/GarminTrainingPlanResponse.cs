namespace FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

public sealed record GarminTrainingPlanResponse
{
    public required List<GarminTrainingPlanTaskItemResponse> TaskList { get; init; }
}
