using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.Models;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.IntervalsICU.Services;

public class IntervalsIcuStorageService(
    IntervalsIcuHttpClient httpClient,
    ILogger<IntervalsIcuStorageService> logger)
{
    private static readonly Dictionary<int, int[]> s_daysDistribution = new()
    {
        {1, [4]},
        {2, [2, 5]},
        {3, [2, 4, 6]},
        {4, [1, 3, 5, 7]},
        {5, [2, 3, 5, 6, 7]},
        {6, [2, 3, 4, 5, 6, 7]},
        {7, [1, 2, 3, 4, 5, 6, 7]}
    };

    public async Task Store(
        IReadOnlyCollection<WhatsOnZwiftToIntervalsIcuConvertResult> items,
        int intervalsIcuFolderId,
        CancellationToken cancellationToken)
    {
        var createWorkoutRequestModelList = CreateCreateWorkoutRequestModels(intervalsIcuFolderId, items).ToList();

        var response = await httpClient.CreateWorkouts(
            Constants.AthleteId, createWorkoutRequestModelList, cancellationToken);

        response.EnsureSuccessStatusCode();
        logger.LogInformation("Stored {ItemsCount} at intervals.icu folder {FolderId}", items.Count, intervalsIcuFolderId);
    }

    private static IEnumerable<WorkoutCreateRequest> CreateCreateWorkoutRequestModels(
        int folderId,
        IReadOnlyCollection<WhatsOnZwiftToIntervalsIcuConvertResult> items)
    {
        var groupsByWeek = items.GroupBy(x => x.FileInfo.Week)
            .OrderBy(x => x.Key.WeekNumber)
            .ToArray();

        if (groupsByWeek.Length == 0)
        {
            yield break;
        }

        var weeksStartsFrom = groupsByWeek[0].Key.WeekNumber;

        foreach (var groupByWeek in groupsByWeek)
        {
            var weekNumber = groupByWeek.Key.WeekNumber;

            foreach (var (index, item) in groupByWeek.Index())
            {
                var dayNumber = item.FileInfo.Day
                    // get day during week based on workouts per week count
                    ?? s_daysDistribution[groupByWeek.Count()][index];

                var absoluteDayNumber = ((weekNumber - weeksStartsFrom) * 7) + dayNumber - 1;

                yield return new WorkoutCreateRequest
                {
                    Name = item.FileInfo.Name,
                    Description = item.IntervalsIcuWorkoutDescription,
                    FolderId = folderId,
                    Day = absoluteDayNumber,
                };
            }
        }
    }
}
