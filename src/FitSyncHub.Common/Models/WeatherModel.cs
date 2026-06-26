namespace FitSyncHub.Common.Models;

public sealed record WeatherModel
{
    public required DateTimeOffset Time { get; init; }
    public required double Temperature { get; init; }
}
