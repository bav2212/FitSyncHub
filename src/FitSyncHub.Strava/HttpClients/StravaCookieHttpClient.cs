using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using FitSyncHub.Functions.JsonSerializerContexts;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Responses.Activities;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Strava.HttpClients;

public class StravaCookieHttpClient : IStravaCookieHttpClient
{
    private const string StravaActivityUrlPattern = "https://www.strava.com/activities/{0}";
    private const string StravaActivitySwapElevationStreamUrlPattern =
        "https://www.strava.com/activities/{0}/swap_elevation_stream?from_source={1}";

    private const string DeviceSource = "device";
#pragma warning disable IDE0051 // Remove unused private members
    private const string LookupSource = "lookup";
#pragma warning restore IDE0051 // Remove unused private members
    private readonly ILogger<StravaCookieHttpClient> _logger;

    public StravaCookieHttpClient(ILogger<StravaCookieHttpClient> logger)
    {
        _logger = logger;
    }

    public async Task<HttpResponseMessage> UpdateActivityVisibilityToOnlyMe(
        ActivityModelResponse activity,
        CookieContainer cookies,
        string authenticityToken,
        Func<DateTime, string> privateNoteFormatter,
        CancellationToken cancellationToken)
    {
        using var httpClient = CreateHttpClient(cookies);

        var url = string.Format(StravaActivityUrlPattern, activity.Id);

        var statsVisibilityMapping = activity.StatsVisibility.ToDictionary(x => x.Type, x => x.Visibility);

        var privateNoteSb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(activity.PrivateNote))
        {
            privateNoteSb.AppendLine(activity.PrivateNote);
        }
        privateNoteSb.AppendLine(privateNoteFormatter(DateTime.UtcNow));

        IEnumerable<KeyValuePair<string, string?>> nameValueCollection = [
            new("utf8", "✓"),
            new("_method", "patch"),
            new("authenticity_token", authenticityToken),
            new("activity[name]", activity.Name),
            new("activity[description]", activity.Description),
            new("activity[perceived_exertion]", activity.PerceivedExertion ?? string.Empty),
            new("activity[prefer_perceived_exertion]", "0"),
            new("activity[private_note]", privateNoteSb.ToString()),
            new("activity[visibility]", "only_me"),
            new("activity[stats_visibility][calories]", statsVisibilityMapping["calories"]),
            new("activity[stats_visibility][heart_rate]", statsVisibilityMapping["heart_rate"]),
            new("activity[stats_visibility][pace]", statsVisibilityMapping["pace"]),
            new("activity[stats_visibility][speed]", statsVisibilityMapping["speed"]),
            new("activity[sport_type]", activity.SportType),
            new("activity[workout_type]", "0"),
            new("activity[commute]", ConvertBoolean(activity.Commute)),
            new("activity[trainer]", ConvertBoolean(activity.Trainer)),
            new("activity[athlete_gear_id]", activity.GearId ?? string.Empty),
            new("commit", "Save"),
        ];
        var content = new FormUrlEncodedContent(nameValueCollection);

        var response = await httpClient.PostAsync(url, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Can not update activity {ActivityId}", activity.Id);
        }
        else
        {
            _logger.LogInformation("Activity {ActivityId} updated to only me", activity.Id);
        }

        return response;
    }

    public async Task<HttpResponseMessage?> CorrectElevationIfNeeded(
        long activityId,
        CookieContainer cookies,
        string authenticityToken,
        CancellationToken cancellationToken)
    {
        using var client = CreateHttpClient(cookies);

        var url = string.Format(StravaActivityUrlPattern, activityId);
        var getActivityResponse = await client.GetAsync(url, cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(await getActivityResponse.Content.ReadAsStringAsync(cancellationToken));

        var tokenNode = doc.DocumentNode.SelectSingleNode("//div[@data-react-class='CorrectElevation']");
        if (tokenNode is null)
        {
            _logger.LogWarning("Can not correct elevation data for activity {ActivityId}. No button for this", activityId);
            return default;
        }

        var correctElevationJsonStringEncoded = tokenNode.GetAttributeValue("data-react-props", "");
        var correctElevationJsonString = HttpUtility.HtmlDecode(correctElevationJsonStringEncoded);
        var correctElevationJson = JsonSerializer.Deserialize(correctElevationJsonString,
            StravaBrowserSessionOnPageJsonSerializerContext.Default.CorrectElevationOnPageModel);

        var needCorrection = correctElevationJson is { }
           && correctElevationJson.LookupExists && correctElevationJson.ActiveSource == DeviceSource;
        if (!needCorrection)
        {
            _logger.LogWarning("Skip {ActivityId}, because it does not need correction", activityId);
            return default;
        }

        var response = await SwapElevation(activityId, authenticityToken, client, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Can not update activity {ActivityId}", activityId);
        }
        else
        {
            _logger.LogInformation("Activity {ActivityId} elevation swapped", activityId);
        }

        return response;
    }

    private static HttpClient CreateHttpClient(CookieContainer cookies)
    {
        var handler = new HttpClientHandler() { CookieContainer = cookies };
        return new HttpClient(handler);
    }

    private static async Task<HttpResponseMessage> SwapElevation(
        long activityId,
        string authenticityToken,
        HttpClient httpClient,
        CancellationToken cancellationToken)
    {
        var url = string.Format(StravaActivitySwapElevationStreamUrlPattern, activityId, DeviceSource);

        IEnumerable<KeyValuePair<string, string?>> nameValueCollection = [
            new("_method", "put"),
            new("authenticity_token", authenticityToken),
        ];
        var content = new FormUrlEncodedContent(nameValueCollection);

        return await httpClient.PostAsync(url, content, cancellationToken);
    }

    private static string ConvertBoolean(bool b) => b ? "1" : "0";
}
