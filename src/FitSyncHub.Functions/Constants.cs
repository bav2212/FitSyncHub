namespace FitSyncHub.Functions;

public static class Constants
{
    public static class CosmosDb
    {
        public const string DatabaseName = "fit-sync-hub";
        public const string ConnectionString = "AzureWebJobsStorageConnectionString";
        public const string LeaseContainerName = "leases";

        public static class Containers
        {
            public const string DistributedCache = "DistributedCache";
            public const string EverestingHOF = "EverestingHOF";
            public const string StravaOAuthData = "StravaOAuthData";
            public const string StravaSummaryActivity = "StravaSummaryActivity";
            public const string StravaWebhookEvent = "StravaWebhookEvent";
        }
    }
}
