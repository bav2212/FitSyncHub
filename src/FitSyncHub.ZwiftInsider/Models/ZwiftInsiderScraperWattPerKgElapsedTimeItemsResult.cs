namespace FitSyncHub.ZwiftInsider.Models;

public record ZwiftInsiderScraperWattPerKgElapsedTimeItemsResult
{
    public required Dictionary<int, double> WattsPerKdTimeEstimate { get; init; } = [];
}
