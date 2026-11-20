namespace FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

public sealed record ActivityStreamsListQueryParams
{
    public string[]? Types { get; init; }
    public bool? IncludeDefaults { get; init; }
}
