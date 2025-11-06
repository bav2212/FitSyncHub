using System.Net.Http.Json;
using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<WellnessResponse> GetWellness(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, StringValues>()
        {
            { "date", date.ToString("yyyy-MM-dd") }
        };

        var requestUri = QueryHelpers.AddQueryString($"{AthleteBaseUrl}/wellness", queryParams);

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuCamelCaseSourceGenerationContext.Default.WellnessResponse)!;
    }

    public async Task<WellnessResponse> UpdateWellness(
        WellnessRequest model,
        CancellationToken cancellationToken)
    {
        var requestUri = $"{AthleteBaseUrl}/wellness/{model.Id}";

        var jsonContent = JsonContent.Create(model, IntervalsIcuCamelCaseSourceGenerationContext.Default.WellnessRequest);
        var response = await _httpClient.PutAsync(requestUri, jsonContent, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuCamelCaseSourceGenerationContext.Default.WellnessResponse)!;
    }
}
