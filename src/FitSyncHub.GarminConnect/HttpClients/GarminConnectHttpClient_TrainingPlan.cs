using System.Text.Json;

namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    public async Task<long> GetActiveTrainingPlanId(
        CancellationToken cancellationToken = default)
    {
        const string Url = "/trainingplan-service/trainingplan/plans?limit=10";

        var response = await _httpClient.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonDocument = JsonDocument.Parse(responseContent);

        return jsonDocument.RootElement.GetProperty("trainingPlanList")
            .EnumerateArray()
            .Where(x =>
                x.GetProperty("trainingType").GetProperty("typeKey").GetString() == "Cycling" &&
                x.GetProperty("trainingPlanCategory").GetString() == "FBT_ADAPTIVE")
            .Select(x => x.GetProperty("trainingPlanId").GetInt64())
            .First();
    }

    public async Task<List<Guid>> GetTrainingPlanCyclingWorkoutGuids(
        long planId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var url = $"/trainingplan-service/trainingplan/fbt-adaptive/{planId}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var jsonDocument = JsonDocument.Parse(responseContent);

        var dateString = date.ToString("yyyy-MM-dd");

        var workoutUuid = jsonDocument.RootElement.GetProperty("taskList")
            .EnumerateArray()
            .Where(x =>
                x.GetProperty("calendarDate").GetString() == dateString &&
                x.GetProperty("taskWorkout").GetProperty("sportType").GetProperty("sportTypeKey")
                    .GetString() == "cycling")
            .Select(x => x.GetProperty("taskWorkout").GetProperty("workoutUuid").GetString())
            .ToList();

        return workoutUuid.ConvertAll(Guid.Parse!);
    }
}
