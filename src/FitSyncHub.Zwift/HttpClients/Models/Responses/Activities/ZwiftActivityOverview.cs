namespace FitSyncHub.Zwift.HttpClients.Models.Responses.Activities;

public sealed record ZwiftActivityOverview
{
    public required long Id { get; init; }
    public required string Sport { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required string Name { get; init; }
    public required string FitFileBucket { get; init; }
    public required string FitFileKey { get; init; }
}
