using System.Text.Json.Serialization;

namespace FitSyncHub.IntervalsICU.HttpClients.Models.Common;

[JsonConverter(typeof(JsonStringEnumConverterSnakeCaseUpper<ActivityMessageType>))]
public enum ActivityMessageType
{
    Text,
    FollowReq,
    CoachReq,
    CoachMeReq,
    Activity,
    Note,
    JoinReq,
    AcceptCoachingGroup,
}
