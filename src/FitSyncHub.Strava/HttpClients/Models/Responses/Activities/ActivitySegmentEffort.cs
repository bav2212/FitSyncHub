namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivitySegmentEffort
{
    public long? Id { get; init; }
    public int? ResourceState { get; init; }
    public string? Name { get; init; }
    public ActivityReference? Activity { get; init; }
    public ActivityAthlete? Athlete { get; init; }
    public int? ElapsedTime { get; init; }
    public int? MovingTime { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? StartDateLocal { get; init; }
    public float? Distance { get; init; }
    public int? StartIndex { get; init; }
    public int? EndIndex { get; init; }
    public float? AverageCadence { get; init; }
    public bool? DeviceWatts { get; init; }
    public float? AverageWatts { get; init; }
    public float? AverageHeartrate { get; init; }
    public float? MaxHeartrate { get; init; }
    public ActivitySegment? Segment { get; init; }
    public int? PrRank { get; init; }
    public ActivityAchievement[]? Achievements { get; init; }
    public string? Visibility { get; init; }
    public int? KomRank { get; init; }
    public bool? Hidden { get; init; }
}
