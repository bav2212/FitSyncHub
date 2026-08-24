namespace FitSyncHub.Zwift.HttpClients.Models.Requests.Activities;

public sealed record ZwiftListActivitiesRequest
{
    public required long ProfileId { get; init; }
    public int Start { get; init; } = 0;
    public int Limit { get; init; } = 0;
}
