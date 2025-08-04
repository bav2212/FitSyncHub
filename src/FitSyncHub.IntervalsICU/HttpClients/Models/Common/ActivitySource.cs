using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ActivitySource>))]
public enum ActivitySource
{
    Strava,
    Upload,
    Manual,
    GarminConnect,
    [JsonStringEnumMemberName("OAUTH_CLIENT")]
    OAuthClient,
    Dropbox,
    Polar,
    Suunto,
    Coros,
    Wahoo,
    Zwift,
}
