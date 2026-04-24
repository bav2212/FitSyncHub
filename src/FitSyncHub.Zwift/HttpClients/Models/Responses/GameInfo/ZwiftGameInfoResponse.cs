namespace FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;

public sealed record ZwiftGameInfoResponse
{
    public required List<ZwiftGameInfoMap> Maps { get; init; }
    //"schedules"
    public required List<ZwiftGameInfoAchievement> Achievements { get; init; }
    //"unlockableCategories"
    //"missions"
    //"challenges"
    //"jerseys"
    //"notableMomentTypes"
    //"trainingPlans"
    public required List<ZwiftGameInfoBikeFrame> BikeFrames { get; init; }
    public required List<ZwiftGameInfoSegment> Segments { get; init; }
}
