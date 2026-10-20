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
    public async Task<IReadOnlyCollection<ActivityMessageResponse>> ListAllMessages(
        string activityId,
        MessagesListQueryParams query,
        CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, StringValues>()
        {
            { "limit",  query.Limit.ToString() },
        };
        if (query.SinceId.HasValue)
        {
            queryParams.Add("sinceId", query.SinceId.Value.ToString());
        }

        var requestUri = QueryHelpers.AddQueryString($"activity/{activityId}/messages", queryParams);

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken)!;
        return JsonSerializer.Deserialize(content, IntervalsIcuSnakeCaseSourceGenerationContext.Default.IReadOnlyCollectionActivityMessageResponse)!;
    }

    public async Task AddMessage(
        string activityId,
        MessageAddRequest model,
        CancellationToken cancellationToken)
    {
        var requestUri = $"activity/{activityId}/messages";

        var jsonContent = JsonContent.Create(model, IntervalsIcuSnakeCaseSourceGenerationContext.Default.MessageAddRequest);
        var response = await _httpClient.PostAsync(requestUri, jsonContent, cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
