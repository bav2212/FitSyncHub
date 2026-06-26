using System.Text.Json;
using FitSyncHub.Weather.HttpClients.Models.Requests;
using FitSyncHub.Weather.HttpClients.Models.Responses;
using FitSyncHub.Weather.JsonSerializerContexts;

namespace FitSyncHub.Weather.HttpClients;

internal sealed partial class CachedOpenMeteoHttpClient : IOpenMeteoHttpClient
{
    private readonly IOpenMeteoHttpClient _openMeteoHttpClient;
    private readonly string _cacheFolderPath;

    public CachedOpenMeteoHttpClient(IOpenMeteoHttpClient openMeteoHttpClient)
    {
        _cacheFolderPath = Path.Combine(Path.GetTempPath(), "OpenMeteoCache");
        _openMeteoHttpClient = openMeteoHttpClient;
    }

    public Task<OpenMeteoResponse> GetOpenMeteoArchive(OpenMeteoRequest request, CancellationToken cancellationToken)
    {
        return Implementation("archive", _openMeteoHttpClient.GetOpenMeteoArchive, request, cancellationToken);
    }

    public Task<OpenMeteoResponse> GetOpenMeteoForecast(OpenMeteoRequest request, CancellationToken cancellationToken)
    {
        return Implementation("forecast", _openMeteoHttpClient.GetOpenMeteoForecast, request, cancellationToken);
    }

    public async Task<OpenMeteoResponse> Implementation(
        string fileNamePrefix,
        Func<OpenMeteoRequest, CancellationToken, Task<OpenMeteoResponse>> getOpenMeteoFunc,
        OpenMeteoRequest request,
        CancellationToken cancellationToken)
    {
        var startDate = request.StartDate;

        // Calculate the first month of the quarter
        var quarterStartMonth = ((startDate.Month - 1) / 3) * 3 + 1;

        var newStartDate = new DateOnly(startDate.Year, quarterStartMonth, 1);
        var newEndDate = newStartDate.AddMonths(3).AddDays(-1);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (newStartDate.AddMonths(3) > today)
        {
            newEndDate = today;
        }

        var newRequest = request with
        {
            StartDate = newStartDate,
            EndDate = newEndDate
        };


        if (!Directory.Exists(_cacheFolderPath))
        {
            Directory.CreateDirectory(_cacheFolderPath);
        }

        List<string> fileNameParts = [
            fileNamePrefix,
            "weather",
            $"from{newRequest.StartDate:yyyy_MM_dd}",
            $"to{newRequest.EndDate:yyyy_MM_dd}",
            $"lat{Math.Round(request.Coordinate.Latitude, 2)}",
            $"lon{Math.Round(request.Coordinate.Longitude, 2)}"
        ];

        var cacheFileName = $"{string.Join('_', fileNameParts)}.json";
        var filePath = Path.Combine(_cacheFolderPath, cacheFileName);

        if (File.Exists(filePath))
        {
            var fileContent = await File.ReadAllTextAsync(filePath, cancellationToken);
            var cachedResponse = JsonSerializer.Deserialize(fileContent, OpenMeteoArchiveGenerationContext.Default.OpenMeteoResponse)!;

            return Convert(request, cachedResponse);
        }

        var response = await getOpenMeteoFunc(newRequest, cancellationToken);
        var serializedResponse = JsonSerializer.Serialize(response, OpenMeteoArchiveGenerationContext.Default.OpenMeteoResponse);
        await File.WriteAllTextAsync(filePath, serializedResponse, cancellationToken);

        return Convert(request, response);
    }

    private static OpenMeteoResponse Convert(OpenMeteoRequest request, OpenMeteoResponse response)
    {
        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(response.Timezone);

        var startDateTimeOfDay = new DateTimeOffset(request.StartDate, TimeOnly.MinValue, TimeSpan.Zero);
        var endDateTimeOfDay = new DateTimeOffset(request.EndDate, TimeOnly.MaxValue, TimeSpan.Zero);

        var trimmedItems = response.Hourly.Time.Index()
            .Where(x =>
            {
                var dateTime = x.Item;
                try
                {
                    var dateTimeOffset = TimeZoneInfo.ConvertTime(dateTime, targetTimeZone);

                    return dateTimeOffset >= startDateTimeOfDay && dateTimeOffset <= endDateTimeOfDay;
                }
                catch (ArgumentException ex)
                {
                    // skip forward and backward time changes
                    return false;
                }
            })
            .ToList();

        var trimmed = trimmedItems.Select(x => x.Index).ToList();
        var range = new Range(trimmed.Min(), trimmed.Max() + 1);

        return response with
        {
            Hourly = response.Hourly with
            {
                Time = [.. response.Hourly.Time[range]],
                Temperature2m = [.. response.Hourly.Temperature2m[range]]
            }
        };
    }
}
