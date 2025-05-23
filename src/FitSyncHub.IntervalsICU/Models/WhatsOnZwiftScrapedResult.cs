namespace FitSyncHub.IntervalsICU.Models;

public record WhatsOnZwiftScrapedResult
{
    public required List<string> NameSegments { get; init; }
    public required List<string> WorkoutList { get; init; }
}
