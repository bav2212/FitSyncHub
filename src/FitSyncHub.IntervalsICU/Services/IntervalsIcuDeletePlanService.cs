using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.IntervalsICU.Services;

public class IntervalsIcuDeletePlanService(
    IntervalsIcuHttpClient httpClient,
    ILogger<IntervalsIcuStorageService> logger)
{
    public async Task DeleteWorkouts(
        int intervalsIcuFolderId,
        CancellationToken cancellationToken)
    {
        var ids = await GetFolderWorkoutIds(intervalsIcuFolderId, cancellationToken);

        foreach (var workoutId in ids)
        {
            await httpClient.DeleteWorkout(workoutId, cancellationToken);
            logger.LogInformation("Deleted workout with id {WorkoutId} from intervals.icu folder {FolderId}", workoutId, intervalsIcuFolderId);
        }
    }

    private async Task<List<int>> GetFolderWorkoutIds(
        int intervalsIcuFolderId,
        CancellationToken cancellationToken)
    {
        var allFoldersResponse = await httpClient
            .ListAllTheAthleteFoldersPlansAndWorkouts(cancellationToken);

        var allFoldersResponseJson = await allFoldersResponse.Content.ReadAsStringAsync(cancellationToken);
        var allFoldersOverviewList = JsonSerializer.Deserialize(allFoldersResponseJson,
            IntervalsIcuSnakeCaseSourceGenerationContext.Default.IReadOnlyCollectionAthleteFolderPlanWorkoutsResponse)!;

        var planOverview = allFoldersOverviewList.Single(x => x.Id == intervalsIcuFolderId);

        return [.. planOverview.Children.Select(x => x.Id)];
    }
}
