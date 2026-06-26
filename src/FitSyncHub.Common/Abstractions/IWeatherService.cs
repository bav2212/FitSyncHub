using FitSyncHub.Common.Models;

namespace FitSyncHub.Common.Abstractions;

public interface IWeatherService
{
    Task<List<WeatherModel>> GetHistoricalWeatherData(
         Coordinate coordinate,
         DateTimeOffset startTime,
         DateTimeOffset endTime,
         CancellationToken cancellationToken);

    Task<List<WeatherModel>> GetForecastedWeatherData(
        Coordinate coordinate,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken);
}
