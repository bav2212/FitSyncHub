namespace FitSyncHub.Strava.HttpClients.Models.Responses.Activities;

public sealed record ActivityGear
{
    public string? Id { get; init; }
    public bool? Primary { get; init; }
    public string? Name { get; init; }
    public string? Nickname { get; init; }
    public int? ResourceState { get; init; }
    public bool? Retired { get; init; }
    public float? Distance { get; init; }
    public float? ConvertedDistance { get; init; }
}
