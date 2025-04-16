using System.Data;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.ZwiftInsider.Services;
public class ZwiftInsiderRoutesService
{
    private readonly ExcelReader _excelReader;
    private readonly ZwiftInsiderScraperService _zwiftInsiderScraper;
    private readonly ILogger<ZwiftInsiderRoutesService> _logger;

    public ZwiftInsiderRoutesService(
        ExcelReader excelReader,
        ZwiftInsiderScraperService zwiftInsiderScraper,
        ILogger<ZwiftInsiderRoutesService> logger)
    {
        _excelReader = excelReader;
        _zwiftInsiderScraper = zwiftInsiderScraper;
        _logger = logger;
    }

    public async Task DoManipulation(string zwiftRoutesFilePath, string sheetName = "Sheet1", CancellationToken cancellationToken = default)
    {
        var dt = _excelReader.Read(zwiftRoutesFilePath, sheetName);

        var result = new List<Result>();

        foreach (DataRow row in dt.Rows)
        {
            var route = row["Route"].ToString()!;
            if (row["2w/kg time"].ToString()!.Length > 0)
            {
                var res = new Result(route,
                    double.Parse(row["lead in length"].ToString()!),
                    double.Parse(row["lead in elevation"].ToString()!),
                    double.Parse(row["2w/kg time"].ToString()!),
                    double.Parse(row["3w/kg time"].ToString()!)
                    );
                result.Add(res);
                continue;
            }

            var link = row[ExcelReader.HyperlinkColumnName]?.ToString() ?? throw new Exception("No hyperlink");
            var scrapeResult = await _zwiftInsiderScraper.ScrapeZwiftInsiderWorkoutPage(new Uri(link), cancellationToken);

            var length = scrapeResult.LeadInAndElevation?.Length ?? 0;
            var elevation = scrapeResult.LeadInAndElevation?.Elevation ?? 0;
            var twoWattsPerKg = scrapeResult.WattPerKg is { }
                && scrapeResult.WattPerKg.WattsPerKdTimeEstimate.TryGetValue(2, out var twoWattPerKgMinutes)
                ? twoWattPerKgMinutes
                : 0;

            var threeWattsPerKg = scrapeResult.WattPerKg is { }
                && scrapeResult.WattPerKg.WattsPerKdTimeEstimate.TryGetValue(3, out var threeWattPerKgMinutes)
                ? threeWattPerKgMinutes
                : 0;

            result.Add(new Result(route, length, elevation, twoWattsPerKg, threeWattsPerKg));
        }

        var resultString = JsonSerializer.Serialize(result);
        _logger.LogInformation("Result: {Result}", resultString);
    }

    private record Result(string Route,
        double Length,
        double Elevation,
        double TwoWattsPerKgMinute,
        double ThreeWattsPerKgMinute);
}
