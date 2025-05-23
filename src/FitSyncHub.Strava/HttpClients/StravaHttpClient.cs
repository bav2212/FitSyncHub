using System.Net.Http.Json;
using System.Text.Json;
using FitSyncHub.Common.Helpers;
using FitSyncHub.Common.Models;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Requests;
using FitSyncHub.Strava.Models.Responses;
using FitSyncHub.Strava.Models.Responses.Activities;
using FitSyncHub.Strava.Models.Responses.Athletes;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace FitSyncHub.Strava.HttpClients;

public class StravaHttpClient : IStravaHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline<UploadActivityResponse> _uploadActivityResiliencePipeline;

    public StravaHttpClient(
        HttpClient httpClient,
        [FromKeyedServices(Constants.ResiliencePipeline.StravaUploadActivityResiliencePipeline)]
        ResiliencePipeline<UploadActivityResponse> uploadActivityResiliencePipeline)
    {
        _httpClient = httpClient;
        _uploadActivityResiliencePipeline = uploadActivityResiliencePipeline;
    }

    public async Task<DetailedAthleteResponse> UpdateAthlete(
        float weight,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsync($"athlete?weight={weight}", null, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content, StravaHttpClientSerializerContext.Default.DetailedAthleteResponse)!;
    }

    public async Task<List<SummaryActivityModelResponse>> GetActivities(
        long before,
        long after,
        int page,
        int perPage,
        CancellationToken cancellationToken)
    {
        var queryParams = new Dictionary<string, string>()
        {
            { "before", before.ToString() },
            { "after", after.ToString() },
            { "page",  page.ToString() },
            { "per_page",  perPage.ToString() }
        };

        var requestUri = $"athlete/activities?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"))}";
        var activities = await _httpClient.GetFromJsonAsync(requestUri,
            StravaHttpClientSerializerContext.Default.ListSummaryActivityModelResponse, cancellationToken);

        return activities!;
    }

    public async Task<ActivityModelResponse> GetActivity(
        long activityId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"activities/{activityId}?include_all_efforts=false";
        var activity = await _httpClient.GetFromJsonAsync(requestUri, StravaHttpClientSerializerContext.Default.ActivityModelResponse, cancellationToken);

        return activity!;
    }

    public async Task<List<SummaryGearResponse>> GetBikes(CancellationToken cancellationToken)
    {
        var detailedAthlete = await _httpClient.GetFromJsonAsync("athlete",
            StravaHttpClientSerializerContext.Default.DetailedAthleteResponse, cancellationToken);

        return detailedAthlete!.Bikes;
    }

    public async Task<ActivityModelResponse> UpdateActivity(
       long activityId,
       UpdatableActivityRequest model,
       CancellationToken cancellationToken)
    {
        var requestUri = $"activities/{activityId}";
        var response = await _httpClient.PutAsJsonAsync(requestUri, model,
            StravaHttpClientSerializerContext.Default.UpdatableActivityRequest, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content, StravaHttpClientSerializerContext.Default.ActivityModelResponse)!;
    }

    public async Task<UploadActivityResponse> UploadStart(
       FileModel file,
       StartUploadActivityRequest model,
       CancellationToken cancellationToken)
    {
        const string RequestUri = "uploads";

        using var formData = FormDataContentHelper.CreateMultipartFormDataContent(file, model,
            StravaHttpClientSerializerContext.Default.StartUploadActivityRequest);

        // Send POST request
        var response = await _httpClient.PostAsync(RequestUri, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content, StravaHttpClientSerializerContext.Default.UploadActivityResponse)!;
    }

    public async Task<UploadActivityResponse> GetUpload(
       long uploadId,
       CancellationToken cancellationToken)
    {
        var requestUri = $"uploads/{uploadId}";

        return await _uploadActivityResiliencePipeline.ExecuteAsync(async ct =>
        {
            var uploadActivityResponse = await _httpClient.GetFromJsonAsync(
                requestUri,
                StravaHttpClientSerializerContext.Default.UploadActivityResponse, ct);

            return uploadActivityResponse!;
        }, cancellationToken);
    }
}
