using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StravaWebhooksAzureFunctions.HttpClients;

public class StravaCookieHttpClient : IStravaCookieHttpClient
{
    private const string StravaUpdateActivityUrlPattern = "https://www.strava.com/activities/{0}";

    public async Task<HttpResponseMessage> UpdateActivityVisibilityToOnlyMe(
        long activityId,
        CookieContainer cookies,
        string authenticityToken,
        CancellationToken cancellationToken)
    {
        var handler = new HttpClientHandler() { CookieContainer = cookies };
        var client = new HttpClient(handler);

        var url = string.Format(StravaUpdateActivityUrlPattern, activityId);

        var content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new ("utf8", "✓"),
            new ("_method", "patch"),
            new ("authenticity_token", authenticityToken),
            new ("activity[name]", "Evening Walk"),
            new ("activity[description]", ""),
            new ("activity[perceived_exertion]", ""),
            new ("activity[prefer_perceived_exertion]", "0"),
            new ("activity[private_note]", ""),
            new ("activity[visibility]", "only_me"),
            new ("activity[stats_visibility][calories]", "everyone"),
            new ("activity[stats_visibility][heart_rate]", "everyone"),
            new ("activity[stats_visibility][pace]", "everyone"),
            new ("activity[stats_visibility][speed]", "everyone"),
            new ("activity[sport_type]", "Walk"),
            new ("activity[workout_type]", "0"),
            new ("activity[commute]", "0"),
            new ("activity[trainer]", "0"),
            new ("activity[trainer]", "1"),
            new ("activity[athlete_gear_id]", ""),
            new ("commit", "Save"),
        });

        return await client.PostAsync(url, content, cancellationToken);
    }
}
