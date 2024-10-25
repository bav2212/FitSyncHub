using FitSyncHub.IntervalsICU.Scrapers;
using FitSyncHub.IntervalsICU.Services;

namespace FitSyncHub.IntervalsICU.Exporters;

public class WorkoutExporter(ZwiftToIntervalsIcuService zwiftToIntervalsIcuService)
{
    public async Task<List<ZwiftToIntervalsIcuConvertResult>> GetPlanWorkoutItems(string planUrl)
    {
        var links = await WhatsOnZwiftScraper.ScrapeWorkoutPlanLinks(planUrl);

        List<ZwiftToIntervalsIcuConvertResult> items = [];

        foreach (var link in links)
        {
            var result = await zwiftToIntervalsIcuService.ScrapeAndConvertToIntervalsIcu(link);
            items.Add(result);
        }

        return items;
    }
}
