using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverter<ActivitySource>))]
public enum ActivitySource
{
    STRAVA,
    UPLOAD,
    MANUAL,
    GARMIN_CONNECT,
    OAUTH_CLIENT,
    DROPBOX,
    POLAR,
    SUUNTO,
    COROS,
    WAHOO,
    ZWIFT
}
