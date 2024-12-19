namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

public class AthleteFolderPlanWorkoutsResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required List<AthleteFolderPlanWorkoutsChildrenResponse> Children { get; init; }
}

public class AthleteFolderPlanWorkoutsChildrenResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}
