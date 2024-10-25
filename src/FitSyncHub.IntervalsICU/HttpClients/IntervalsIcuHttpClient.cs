
using System.Net.Http.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using ZwiftToIntervalsICUConverter.HttpClients.Models;

namespace FitSyncHub.IntervalsICU.HttpClients;

public class IntervalsIcuHttpClient(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<HttpResponseMessage> CreateWorkouts(
        string athleteId, IReadOnlyCollection<CreateWorkoutRequestModel> model, CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{athleteId}/workouts/bulk";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSourceGenerationContext.Default.IReadOnlyCollectionCreateWorkoutRequestModel);
        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

        response.EnsureSuccessStatusCode();

        return response;
    }

    public async Task<HttpResponseMessage> CreateWorkout(
        string athleteId, CreateWorkoutRequestModel model, CancellationToken cancellationToken)
    {
        var url = $"api/v1/athlete/{athleteId}/workouts";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSourceGenerationContext.Default.CreateWorkoutRequestModel);
        var response = await _httpClient.PostAsync(url, jsonContent, cancellationToken);

        response.EnsureSuccessStatusCode();

        return response;
    }
}