using System.Globalization;
using System.Net.Http.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using ZwiftToIntervalsICUConverter.HttpClients.Models;

namespace FitSyncHub.IntervalsICU.HttpClients;

public class IntervalsIcuHttpClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<HttpResponseMessage> ListActivities(
        string athleteId,
        DateTime oldest,
        DateTime newest,
        int limit,
        CancellationToken cancellationToken)
    {
        var baseUrl = $"api/v1/athlete/{athleteId}/activities";

        var queryParams = new Dictionary<string, string>()
        {
            { "oldest", oldest.ToString("s", CultureInfo.InvariantCulture) },
            { "newest", newest.ToString("s", CultureInfo.InvariantCulture) },
            { "limit",  limit.ToString() },
        };

        var requestUri = $"{baseUrl}?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return response;
    }

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
        IReadOnlyCollection<CreateWorkoutRequestModel> model,
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{athleteId}/workouts/bulk";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSourceGenerationContext.Default.IReadOnlyCollectionCreateWorkoutRequestModel);
        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

        response.EnsureSuccessStatusCode();

        return response;
    }

    public async Task<HttpResponseMessage> CreateWorkout(
        string athleteId,
        CreateWorkoutRequestModel model,
        CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{athleteId}/workouts";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSourceGenerationContext.Default.CreateWorkoutRequestModel);
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
