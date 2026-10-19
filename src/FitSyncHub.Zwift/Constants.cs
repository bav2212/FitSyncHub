namespace FitSyncHub.Zwift;

internal static class Constants
{
    public const string ZwiftAuthResiliencePipeline = nameof(ZwiftAuthResiliencePipeline);
    public const string ZwiftHttpClientJson = nameof(ZwiftHttpClientJson);
    public const string ZwiftHttpClientProto = nameof(ZwiftHttpClientProto);

    public const string ZwiftClimbPortalWorldName = "Climb Portal";

    public static uint WorldIdToCourseId(uint worldId) => worldId switch
    {
        1 => 6,
        2 => 2,
        3 => 7,
        4 => 8,
        5 => 9,
        6 => 10,
        7 => 11,
        8 => 12,
        9 => 13,
        10 => 14,
        11 => 15,
        12 => 16,
        13 => 17,
        _ => throw new NotImplementedException()
    };

}
