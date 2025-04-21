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
using Microsoft.Extensions.Logging;
using Polly;

namespace FitSyncHub.Strava.HttpClients;

public class StravaRestHttpClient : IStravaRestHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<StravaRestHttpClient> _logger;

    public StravaRestHttpClient(
        HttpClient httpClient,
        ILogger<StravaRestHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<DetailedAthleteResponse> UpdateAthlete(
        float weight,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.PutAsync($"athlete?weight={weight}", null, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content, StravaRestApiSerializerContext.Default.DetailedAthleteResponse)!;
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
            StravaRestApiSerializerContext.Default.ListSummaryActivityModelResponse, cancellationToken);

        return activities!;
    }

    public async Task<ActivityModelResponse> GetActivity(
        long activityId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"activities/{activityId}?include_all_efforts=false";
        var activity = await _httpClient.GetFromJsonAsync(requestUri, StravaRestApiSerializerContext.Default.ActivityModelResponse, cancellationToken);

        return activity!;
    }

    public async Task<List<SummaryGearResponse>> GetBikes(CancellationToken cancellationToken)
    {
        var detailedAthlete = await _httpClient.GetFromJsonAsync("athlete",
            StravaRestApiSerializerContext.Default.DetailedAthleteResponse, cancellationToken);

        return detailedAthlete!.Bikes;
    }

    public async Task<ActivityModelResponse> UpdateActivity(
       long activityId,
       UpdatableActivityRequest model,
       CancellationToken cancellationToken)
    {
        var requestUri = $"activities/{activityId}";
        var response = await _httpClient.PutAsJsonAsync(requestUri, model,
            StravaBrowserSessionSerializerContext.Default.UpdatableActivityRequest, cancellationToken);

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content, StravaRestApiSerializerContext.Default.ActivityModelResponse)!;
    }

    public async Task<UploadActivityResponse> UploadStart(
       FileModel file,
       StartUploadActivityRequest model,
       CancellationToken cancellationToken)
    {
        const string RequestUri = "uploads";

        using var formData = FormDataContentHelper
            .CreateMultipartFormDataContent(file, model, StravaRestApiSerializerContext.Default.StartUploadActivityRequest);

        // Send POST request
        var response = await _httpClient.PostAsync(RequestUri, formData, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize(content, StravaRestApiSerializerContext.Default.UploadActivityResponse)!;
    }

    public async Task<UploadActivityResponse> GetUpload(
       long uploadId,
       CancellationToken cancellationToken)
    {
        const int RetryCount = 3;
        const int InitialWaitDurationMilliseconds = 4 * 1000;

        var requestPolicy = Policy
            .HandleResult<UploadActivityResponse>(r => r.Status == "Your activity is still being processed.")
            .WaitAndRetryAsync(
                retryCount: RetryCount,
                retryAttempt => TimeSpan.FromMilliseconds(InitialWaitDurationMilliseconds * Math.Pow(2, retryAttempt - 1)),
                (uploadResponseDelegate, timespan, retryAttempt, _) =>
                {
                    var uploadResponse = uploadResponseDelegate.Result;

                    _logger.LogInformation("Upload status: {Status}, Error: {Error}, retry {RetryCount} after {Timespan}",
                        uploadResponse.Status,
                        uploadResponse.Error,
                        retryAttempt,
                        timespan);
                });


        var requestUri = $"uploads/{uploadId}";

        return await requestPolicy.ExecuteAsync(async () =>
        {
            var uploadActivityResponse = await _httpClient.GetFromJsonAsync(requestUri,
                StravaRestApiSerializerContext.Default.UploadActivityResponse, cancellationToken);
            return uploadActivityResponse!;
        });
    }
}
