namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivityLap
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
    public float? AverageSpeed { get; init; }
    public float? MaxSpeed { get; init; }
    public int? LapIndex { get; init; }
    public int? Split { get; init; }
    public int? StartIndex { get; init; }
    public int? EndIndex { get; init; }
    public float? TotalElevationGain { get; init; }
    public float? AverageCadence { get; init; }
    public bool? DeviceWatts { get; init; }
    public float? AverageWatts { get; init; }
    public float? AverageHeartrate { get; init; }
    public float? MaxHeartrate { get; init; }
}
