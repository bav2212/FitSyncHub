namespace FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

public record GarminTrainingPlanResponse
{
    public required List<GarminTrainingPlanTaskItemResponse> TaskList { get; init; }
}
