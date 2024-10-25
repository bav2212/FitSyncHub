namespace FitSyncHub.IntervalsICU.Parsers;

public class ParsedZwiftWorkoutFtpRange : IParsedZwiftWorkoutFtp
{
    public required int From { get; init; }
    public required int To { get; init; }
}
