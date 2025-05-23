using System.Net.Http.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<HttpResponseMessage> ListAllTheAthleteFoldersPlansAndWorkouts(
        string athleteId,
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{athleteId}/folders";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return response;
    }

    public async Task<HttpResponseMessage> CreateWorkouts(
        string athleteId,
        IReadOnlyCollection<WorkoutCreateRequest> model,
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{athleteId}/workouts/bulk";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSnakeCaseSourceGenerationContext.Default.IReadOnlyCollectionWorkoutCreateRequest);
        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

        response.EnsureSuccessStatusCode();

        return response;
    }

    public async Task<HttpResponseMessage> CreateWorkout(
        string athleteId,
        WorkoutCreateRequest model,
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{athleteId}/workouts";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSnakeCaseSourceGenerationContext.Default.WorkoutCreateRequest);
        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

        response.EnsureSuccessStatusCode();

        return response;
    }

    public async Task<HttpResponseMessage> DeleteWorkout(
        string athleteId,
        int workoutId,
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{athleteId}/workouts/{workoutId}";

        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return response;
    }
}
