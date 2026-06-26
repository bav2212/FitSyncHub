using Microsoft.Extensions.Options;

namespace FitSyncHub.Weather.Options;

public sealed record WeatherOptions : IOptions<WeatherOptions>
{
    WeatherOptions IOptions<WeatherOptions>.Value => this;
}
