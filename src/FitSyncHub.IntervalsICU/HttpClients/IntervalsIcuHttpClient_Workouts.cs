using System.Net.Http.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<HttpResponseMessage> ListAllTheAthleteFoldersPlansAndWorkouts(
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{_athleteId}/folders";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return response;
    }

    public async Task<HttpResponseMessage> CreateWorkouts(
        IReadOnlyCollection<WorkoutCreateRequest> model,
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{_athleteId}/workouts/bulk";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSnakeCaseSourceGenerationContext.Default.IReadOnlyCollectionWorkoutCreateRequest);
        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

        response.EnsureSuccessStatusCode();

        return response;
    }

    public async Task<HttpResponseMessage> CreateWorkout(
        WorkoutCreateRequest model,
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{_athleteId}/workouts";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSnakeCaseSourceGenerationContext.Default.WorkoutCreateRequest);
        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

        response.EnsureSuccessStatusCode();

        return response;
    }

    // can change workoutId to DeleteWorkoutRequest if needed to pass 'others' query param
    public async Task<HttpResponseMessage> DeleteWorkout(
        int workoutId,
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{_athleteId}/workouts/{workoutId}";

        var response = await _httpClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return response;
    }
}
