using System.Net;
using System.Text;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.HttpClients.Models.Responses.Activities;

namespace StravaWebhooksAzureFunctions.HttpClients;

public class StravaCookieHttpClient : IStravaCookieHttpClient
{
    private const string _stravaUpdateActivityUrlPattern = "https://www.strava.com/activities/{0}";

    public async Task<HttpResponseMessage> UpdateActivityVisibilityToOnlyMe(
        ActivityModelResponse activity,
        CookieContainer cookies,
        string authenticityToken,
        Func<DateTime, string> privateNoteFormatter,
        CancellationToken cancellationToken)
    {
        var handler = new HttpClientHandler() { CookieContainer = cookies };
        var client = new HttpClient(handler);

        var url = string.Format(_stravaUpdateActivityUrlPattern, activity.Id);

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

        return await client.PostAsync(url, content, cancellationToken);
    }

    private static string ConvertBoolean(bool b) => b ? "1" : "0";
}
