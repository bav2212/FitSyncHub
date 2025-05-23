namespace FitSyncHub.IntervalsICU.Options;

public record IntervalsIcuOptions
{
    public const string Position = "IntervalsICU";

    public required string ApiKey { get; init; }
    public required string AthleteId { get; init; }
}
