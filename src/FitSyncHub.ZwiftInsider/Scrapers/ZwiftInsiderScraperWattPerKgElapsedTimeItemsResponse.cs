namespace FitSyncHub.ZwiftInsider.Scrapers;

public record ZwiftInsiderScraperWattPerKgElapsedTimeItemsResponse
{
    public required Dictionary<int, double> WattsPerKdTimeEstimate { get; init; } = [];
}
