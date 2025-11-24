namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public sealed record AthleteFolderPlanWorkoutsResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required List<AthleteFolderPlanWorkoutsChildrenResponse> Children { get; init; }
}

public sealed record AthleteFolderPlanWorkoutsChildrenResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
