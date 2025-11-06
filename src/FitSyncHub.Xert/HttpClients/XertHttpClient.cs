using System.Net.Http.Json;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.Xert.Abstractions;
using FitSyncHub.Xert.Models.Responses;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Xert.HttpClients;

public class XertHttpClient : IXertHttpClient
{
    private readonly HttpClient _httpClient;

    public XertHttpClient(
        HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<TrainingInfoResponse> GetTrainingInfo(XertWorkoutFormat format, CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, StringValues>
        {
            { "format", format.ToString() }
        };
        var requestUri = QueryHelpers.AddQueryString("oauth/training_info", queryParams);

        var response = await _httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(XertHttpClientSerializerContext.Default.TrainingInfoResponse,
            cancellationToken) ?? throw new InvalidDataException();
    }

    public async Task<string> GetDownloadWorkout(string downloadUrl, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(downloadUrl, cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
