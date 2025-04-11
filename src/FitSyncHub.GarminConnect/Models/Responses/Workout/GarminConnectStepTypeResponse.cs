namespace FitSyncHub.GarminConnect.Models.Responses.Workout;

public record GarminConnectStepTypeResponse
{
    public int StepTypeId { get; init; }
    public string StepTypeKey { get; init; } = default!;
    public int DisplayOrder { get; init; }
}
