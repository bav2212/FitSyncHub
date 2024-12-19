using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients;
using Microsoft.Extensions.Logging;
using ZwiftToIntervalsICUConverter.HttpClients.Models;

namespace FitSyncHub.IntervalsICU.Services;

public class IntervalsIcuDeletePlanService(
    IntervalsIcuHttpClient httpClient,
    ILogger<IntervalsIcuStorageService> logger)
{
    public async Task DeleteWorkouts(
        int intervalsIcuFolderId,
        CancellationToken cancellationToken)
    {
        var athleteId = Constants.AthleteId;
        var ids = await GetFolderWorkoutIds(athleteId, intervalsIcuFolderId, cancellationToken);

        foreach (var workoutId in ids)
        {
            await httpClient.DeleteWorkout(athleteId, workoutId, cancellationToken);
            logger.LogInformation("Deleted workout with id {WorkoutId} from intervals.icu folder {FolderId}", workoutId, intervalsIcuFolderId);
        }
    }

    private async Task<List<int>> GetFolderWorkoutIds(
        string athleteId,
        int intervalsIcuFolderId,
        CancellationToken cancellationToken)
    {
        var allFoldersResponse = await httpClient
            .ListAllTheAthleteFoldersPlansAndWorkouts(athleteId, cancellationToken);

        var allFoldersResponseJson = await allFoldersResponse.Content.ReadAsStringAsync(cancellationToken);
        var allFoldersOverviewList = JsonSerializer.Deserialize(allFoldersResponseJson,
            IntervalsIcuSourceGenerationContext.Default.IReadOnlyCollectionAthleteFolderPlanWorkoutsResponse)!;

        var planOverview = allFoldersOverviewList.Single(x => x.Id == intervalsIcuFolderId);

        return planOverview.Children.Select(x => x.Id).ToList();
    }
}
