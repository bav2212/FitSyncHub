using FitSyncHub.Common.Abstractions;
using FitSyncHub.Common.Models;
using FitSyncHub.Weather.HttpClients;
using FitSyncHub.Weather.HttpClients.Models.Requests;
using FitSyncHub.Weather.HttpClients.Models.Responses;

namespace FitSyncHub.Weather.Services;

internal sealed class OpenMeteoWeatherService : IWeatherService
{
    private readonly IOpenMeteoHttpClient _openMeteoHttpClient;

    public OpenMeteoWeatherService(IOpenMeteoHttpClient openMeteoHttpClient)
    {
        _openMeteoHttpClient = openMeteoHttpClient;
    }

    public async Task<List<WeatherModel>> GetHistoricalWeatherData(
        Coordinate coordinate,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken)
    {
        var request = new OpenMeteoRequest
        {
            Coordinate = coordinate,
            StartDate = DateOnly.FromDateTime(startTime.DateTime),
            EndDate = DateOnly.FromDateTime(endTime.DateTime),
        };

        var openMeteoArchive = await _openMeteoHttpClient.GetOpenMeteoArchive(request, cancellationToken);
        return ConvertAndFilter(openMeteoArchive, startTime, endTime);
    }

    public async Task<List<WeatherModel>> GetForecastedWeatherData(
        Coordinate coordinate,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        CancellationToken cancellationToken)
    {
        var request = new OpenMeteoRequest
        {
            Coordinate = coordinate,
            StartDate = DateOnly.FromDateTime(startTime.DateTime),
            EndDate = DateOnly.FromDateTime(endTime.DateTime),
        };

        var openMeteoArchive = await _openMeteoHttpClient.GetOpenMeteoForecast(request, cancellationToken);
        return ConvertAndFilter(openMeteoArchive, startTime, endTime);
    }

    private static List<WeatherModel> ConvertAndFilter(
        OpenMeteoResponse openMeteoResponse,
        DateTimeOffset startTime,
        DateTimeOffset endTime)
    {
        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(openMeteoResponse.Timezone);

        var zippedWeatherData = openMeteoResponse.Hourly.Time
            .Zip(openMeteoResponse.Hourly.Temperature2m)
            .Select(x => new WeatherModel
            {
                Time = (DateTimeOffset)TimeZoneInfo.ConvertTime(x.First, targetTimeZone).ToUniversalTime(),
                Temperature = x.Second,
            }).ToList();

        var startTimeHourFloor = new DateTimeOffset(startTime.Year, startTime.Month, startTime.Day, startTime.Hour, 0, 0, startTime.Offset);
        var endTimeHourCeiling = new DateTimeOffset(endTime.Year, endTime.Month, endTime.Day, endTime.Hour + 1, 0, 0, endTime.Offset);

        return [.. zippedWeatherData.Where(x => x.Time >= startTimeHourFloor && x.Time <= endTimeHourCeiling)];
    }
}
