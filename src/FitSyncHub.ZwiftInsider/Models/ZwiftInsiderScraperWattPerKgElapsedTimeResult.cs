namespace FitSyncHub.ZwiftInsider.Models;

public record ZwiftInsiderScraperWattPerKgElapsedTimeResult
{
    public required int WattPerKg { get; init; }
    public required double Minutes { get; init; }
}
