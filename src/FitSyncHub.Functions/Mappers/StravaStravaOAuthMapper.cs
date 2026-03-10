using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Strava.Models;
using Riok.Mapperly.Abstractions;

namespace FitSyncHub.Functions.Mappers;

[Mapper]
internal sealed partial class StravaStravaOAuthMapper
{
    [MapProperty(
        nameof(StravaOAuthTokenModel.AthleteId),
        nameof(StravaOAuthData.Id),
        Use = nameof(MapId))]
    public partial StravaOAuthData StravaOAuthTokenToDataModel(StravaOAuthTokenModel oAuthTokenModel);

    // set Default = false to not use it for all long => string conversions
    [UserMapping(Default = false)]
    public static string MapId(long athleteId) => athleteId.ToString();

    [MapperIgnoreSource(nameof(StravaOAuthData.Id))]
    public partial StravaOAuthTokenModel DataModelToStravaOAuthToken(StravaOAuthData dataModel);
}
