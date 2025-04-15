namespace FitSyncHub.IntervalsICU.Models;

public record WhatsOnZwiftToIntervalsIcuConvertFileInfo
{
    public required string Name { get; init; }
    public required WhatsOnZwiftToIntervalsIcuWeek Week { get; init; }
    public required int? Day { get; init; }
}
