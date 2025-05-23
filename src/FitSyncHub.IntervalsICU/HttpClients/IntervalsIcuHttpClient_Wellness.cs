using System.Net.Http.Json;
using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<WellnessResponse> GetWellness(
        string athleteId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var requestUri = $"api/v1/athlete/{athleteId}/wellness/{date:yyyy-MM-dd}";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuCamelCaseSourceGenerationContext.Default.WellnessResponse)!;
    }

    public async Task<WellnessResponse> UpdateWellness(
        string athleteId,
        WellnessRequest model,
        CancellationToken cancellationToken)
    {
        var requestUri = $"api/v1/athlete/{athleteId}/wellness/{model.Id}";

        var jsonContent = JsonContent.Create(model, IntervalsIcuCamelCaseSourceGenerationContext.Default.WellnessRequest);
        var response = await _httpClient.PutAsync(requestUri, jsonContent, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuCamelCaseSourceGenerationContext.Default.WellnessResponse)!;
    }
}
