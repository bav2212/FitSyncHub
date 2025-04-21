using System.Text;

namespace FitSyncHub.Common.Applications.IntervalsIcu.Models;

public record IntervalsIcuRideWorkoutLine : IIntervalsIcuWorkoutLine
{
    public required TimeSpan Time { get; init; }
    public required int? Rpm { get; init; }
    public required IIntervalsIcuWorkoutFtp? Ftp { get; init; }
    public required bool IsMaxEffort { get; init; }
    public required bool IsFreeRide { get; init; }

    public string ConvertToIntervalsIcuFormat()
    {
        var sb = new StringBuilder("-");
        AppendTimeSegment(sb);

        if (IsFreeRide)
        {
            sb.Append(" freeride");
            return sb.ToString();
        }

        if (IsMaxEffort)
        {
            // maxeffort doesn't visible on chart + not calculated in TSS, so adding Z6 as hack
            sb.Append(" maxeffort Z6");
            return sb.ToString();
        }

        if (Rpm is { } rpm)
        {
            sb.Append($" {rpm}rpm");
        }

        AppendFtpSegment(sb);
        return sb.ToString();
    }

    private void AppendTimeSegment(StringBuilder sb)
    {
        sb.Append(' ');
        if (Time.Hours != 0)
        {
            sb.Append($"{Time.Hours}h");
        }

        if (Time.Minutes != 0)
        {
            sb.Append($"{Time.Minutes}m");
        }

        if (Time.Seconds != 0)
        {
            sb.Append($"{Time.Seconds}s");
        }
    }

    private void AppendFtpSegment(StringBuilder sb)
    {
        if (Ftp is null)
        {
            return;
        }

        sb.Append(' ');
        if (Ftp is IntervalsIcuWorkoutFtpRange ftpRange)
        {
            if (ftpRange.IsRampRange)
            {
                sb.Append("ramp ");
            }

            sb.Append($"{ftpRange.From}-{ftpRange.To}%");
            return;
        }

        if (Ftp is IntervalsIcuWorkoutFtpSingle ftpSingle)
        {
            sb.Append($"{ftpSingle.Value}%");
            return;
        }

        throw new NotImplementedException();
    }
}
