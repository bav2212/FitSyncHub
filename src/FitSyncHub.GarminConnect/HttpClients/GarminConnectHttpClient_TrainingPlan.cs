using System.Text.Json;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Models.Responses.TrainingPlan;

namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    public async Task<long> GetActiveTrainingPlanId(
        CancellationToken cancellationToken)
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

    public async Task<GarminTrainingPlanResponse> GetTrainingPlan(
        long planId,
        CancellationToken cancellationToken)
    {
        var url = $"/trainingplan-service/trainingplan/fbt-adaptive/{planId}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(responseContent, GarminConnectTrainingPlanSerializerContext.Default.GarminTrainingPlanResponse)!;
    }
}
