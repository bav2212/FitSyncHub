using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.IntervalsICU.Services;

public class IntervalsIcuDeletePlanService(
    IntervalsIcuHttpClient httpClient,
    IOptions<IntervalsIcuOptions> options,
    ILogger<IntervalsIcuStorageService> logger)
{
    private readonly string _athleteId = options.Value.AthleteId;

    public async Task DeleteWorkouts(
        int intervalsIcuFolderId,
        CancellationToken cancellationToken)
    {
        var ids = await GetFolderWorkoutIds(_athleteId, intervalsIcuFolderId, cancellationToken);

        foreach (var workoutId in ids)
        {
            await httpClient.DeleteWorkout(_athleteId, workoutId, cancellationToken);
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
            IntervalsIcuSnakeCaseSourceGenerationContext.Default.IReadOnlyCollectionAthleteFolderPlanWorkoutsResponse)!;

        var planOverview = allFoldersOverviewList.Single(x => x.Id == intervalsIcuFolderId);

        return [.. planOverview.Children.Select(x => x.Id)];
    }
}
