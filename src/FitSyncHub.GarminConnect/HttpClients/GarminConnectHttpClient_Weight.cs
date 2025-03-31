using System.Net.Http.Json;
using System.Text.Json;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Models.Requests;
using FitSyncHub.GarminConnect.Models.Responses;

namespace FitSyncHub.GarminConnect.HttpClients;

public partial class GarminConnectHttpClient
{
    public async Task<GarminWeightResponse> GetWeightDayView(DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var url = $"/weight-service/weight/dayview/{date:yyyy-MM-dd}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize(content, GarminConnectCamelCaseSerializerContext.Default.GarminWeightResponse)!;
        return result;
    }

    public async Task SetUserWeight(
        double weight,
        CancellationToken cancellationToken = default)
    {
        var body = new GarminSetUserWeightRequest
        {
            UserData = new WeightUserData
            {
                Weight = weight
            }
        };

        const string Url = "/userprofile-service/userprofile/user-settings";

        var response = await _httpClient.PutAsJsonAsync(Url, body,
            GarminConnectCamelCaseSerializerContext.Default.GarminSetUserWeightRequest, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
