namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public record WorkoutCreateRequest
{
    public required long FolderId { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public string Type { get; init; } = "Ride";
    public bool Indoor { get; init; } = true;
    public string[] Targets { get; init; } = ["POWER"];
    public required int Day { get; init; }
}
