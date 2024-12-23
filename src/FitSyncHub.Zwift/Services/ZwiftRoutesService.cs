using System.Data;
using System.Text.Json;
using FitSyncHub.Zwift.Scrapers;

namespace FitSyncHub.Zwift.Services;
public class ZwiftRoutesService
{
    private readonly ExcelReader _excelReader;

    public ZwiftRoutesService(ExcelReader excelReader)
    {
        _excelReader = excelReader;
    }

    public async Task DoManipulation()
    {
        var dt = _excelReader.Read(@"C:\Users\bav22\OneDrive\Рабочий стол\zwift_routes.xlsx", "Sheet1");

        var result = new List<Result>();

        foreach (DataRow row in dt.Rows)
        {
            var route = row["Route"].ToString()!;

            if (row["lead in length"].ToString().Length > 0)
            {
                var res = new Result(route,
                    double.Parse(row["lead in length"].ToString()),
                    double.Parse(row["lead in elevation"].ToString()));
                result.Add(res);
                continue;
            }

            var link = row[ExcelReader.HyperlinkColumnName].ToString();
            var scrapeResult = await ZwiftInsiderScraper.ScrapeZwiftInsiderWorkoutPage(new Uri(link));
            result.Add(new Result(route, scrapeResult.Length, scrapeResult.Elevation));
        }

        var resultString = JsonSerializer.Serialize(result);
        Console.WriteLine(resultString);
    }

    private record Result(string Route, double Length, double Elevation);
}
