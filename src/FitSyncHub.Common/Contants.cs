namespace FitSyncHub.Common;

public static class Constants
{
    public static class CacheKeys
    {
        public const string GarminLastWeightResponse = "garmin-last-weight-response";
        public const string LactateLastSyncedDate = "lactate-last-synced-date";
        public const string XertOauth2TokenModel = "xert-oauth2-token-model";
        public const string ZwiftAuthTokenModel = "zwift-auth-token-model";
        public const string GarminAuthenticationTokenModel = "garmin-oauth2-token-model";
        public const string GarminMfaClientState = "garmin-oauth2-mfa-client-state";
        public const string ZwiftRacingAuthCookie = "zwift-racing-auth-cookie";
        public const string FlammeRougeRacingTourRegisteredRiderIdsPrefix = "frr-tour-registered-rider-ids";
    }
}
