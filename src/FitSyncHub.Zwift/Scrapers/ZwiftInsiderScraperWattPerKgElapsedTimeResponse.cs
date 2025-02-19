namespace FitSyncHub.ZwiftInsider.Scrapers;

public record ZwiftInsiderScraperWattPerKgElapsedTimeResponse
{
    public required int WattPerKg { get; init; }
    public required double Minutes { get; init; }
}

