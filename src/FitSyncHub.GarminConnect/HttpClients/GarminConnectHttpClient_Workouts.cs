using System.Text.Json;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Models.Responses.Workout;

namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    public async Task<GarminConnectWorkoutResponse> GetWorkout(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var url = $"workout-service/fbt-adaptive/{id}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content,
            GarminConnectWorkoutSerializerContext.Default.GarminConnectWorkoutResponse)!;
    }
}
