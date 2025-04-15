namespace FitSyncHub.IntervalsICU.Models;

public record WhatsOnZwiftToIntervalsIcuConvertResult
{
    public required string IntervalsIcuWorkoutDescription { get; init; }
    public required WhatsOnZwiftToIntervalsIcuConvertFileInfo FileInfo { get; init; }
}
