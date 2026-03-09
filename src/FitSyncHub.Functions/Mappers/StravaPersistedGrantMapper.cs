using FitSyncHub.Functions.Data.Entities;
using FitSyncHub.Strava.Models;
using Riok.Mapperly.Abstractions;

namespace FitSyncHub.Functions.Mappers;

[Mapper]
internal sealed partial class StravaPersistedGrantMapper
{
    [MapProperty(
        nameof(StravaOAuthTokenModel.AthleteId),
        nameof(PersistedGrant.Id),
        Use = nameof(MapId))]
    public partial PersistedGrant StravaOAuthTokenToPersistedGrant(StravaOAuthTokenModel oAuthTokenModel);

    // set Default = false to not use it for all long => string conversions
    [UserMapping(Default = false)]
    public static string MapId(long athleteId) => athleteId.ToString();

    [MapperIgnoreSource(nameof(PersistedGrant.Id))]
    public partial StravaOAuthTokenModel PersistedGrantToStravaOAuthToken(PersistedGrant persistedGrant);
}
