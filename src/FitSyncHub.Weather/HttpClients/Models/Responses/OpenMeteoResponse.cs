using System.Text.Json.Serialization;

namespace FitSyncHub.Weather.HttpClients.Models.Responses;

public sealed record OpenMeteoResponse
{
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required double GenerationtimeMs { get; init; }
    public required int UtcOffsetSeconds { get; init; }
    public required string Timezone { get; init; }
    public required string TimezoneAbbreviation { get; init; }
    public required double Elevation { get; init; }
    public required OpenMeteoHourlyUnits HourlyUnits { get; init; }
    public required OpenMeteoHourly Hourly { get; init; }
}

public sealed record OpenMeteoHourlyUnits
{
    public required string Time { get; init; }
    [JsonPropertyName("temperature_2m")]
    public required string Temperature2m { get; init; }
}

public sealed record OpenMeteoHourly
{
    public required DateTimeOffset[] Time { get; init; }
    [JsonPropertyName("temperature_2m")]
    public required double[] Temperature2m { get; init; }
}
