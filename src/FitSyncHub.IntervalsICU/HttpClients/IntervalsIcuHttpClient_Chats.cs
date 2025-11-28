using System.Net.Http.Json;
using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace FitSyncHub.IntervalsICU.HttpClients;

public partial class IntervalsIcuHttpClient
{
    public async Task<IReadOnlyCollection<ActivityMessageResponse>> ListAllMessages(
        string activityId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"activity/{activityId}/messages";

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.IReadOnlyCollectionActivityMessageResponse)!;
    }

    public async Task AddMessage(
        string activityId,
        AddMessageRequest model,
        CancellationToken cancellationToken)
    {
        var requestUri = $"activity/{activityId}/messages";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSnakeCaseSourceGenerationContext.Default.AddMessageRequest);
        var response = await _httpClient.PostAsync(requestUri, jsonContent, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
