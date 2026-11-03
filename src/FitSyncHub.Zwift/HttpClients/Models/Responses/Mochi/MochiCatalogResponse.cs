namespace FitSyncHub.Zwift.HttpClients.Models.Responses.Mochi;

public sealed record MochiCatalogResponse
{
    public IReadOnlyList<MochiCollectionResponse> Collections { get; init; } = [];
    public MochiAssetResponse? UpsellPreviewImage { get; init; }
}

public sealed record MochiCollectionResponse
{
    public string? Name { get; init; }
    public IReadOnlyList<MochiRideResponse> Rides { get; init; } = [];
}

public sealed record MochiRideResponse
{
    public string? Name { get; init; }
    public string? PrettyName { get; init; }
    public long RoadId { get; init; }
    public int CountryCode { get; init; }
    public string? Source { get; init; }
    public IReadOnlyList<double> MinimapAnchor { get; init; } = [];
    public double MinimapTileScale { get; init; }
    public double CameraHeight { get; init; }
    public double SrcStart { get; init; }
    public double SrcEnd { get; init; }
    public IReadOnlyList<MochiSpinnerTriggerResponse> SpinnerTriggers { get; init; } = [];
    public double LightIntensity { get; init; }
    public double AmbientLightIntensity { get; init; }
    public MochiSunPositionResponse? SunStart { get; init; }
    public MochiSunPositionResponse? SunEnd { get; init; }
    public IReadOnlyList<MochiAssetResponse> MinimapTiles { get; init; } = [];
    public IReadOnlyList<MochiAssetResponse> MinimapTilesAndroidTgax { get; init; } = [];
    public IReadOnlyList<MochiAssetResponse> MinimapTilesIosTgax { get; init; } = [];
    public IReadOnlyList<MochiAssetResponse> MinimapTilesMacpcZtx { get; init; } = [];
    public int MinimapTileOffset { get; init; }
    public double MinimapOriginLatitude { get; init; }
    public double MinimapOriginLongitude { get; init; }
    public MochiAssetResponse? Thumbnail { get; init; }
    public IReadOnlyList<MochiResolutionResponse> Resolutions { get; init; } = [];
    public int Crf { get; init; }
    public string? RouteHash { get; init; }
    public MochiAssetResponse? Road { get; init; }
    public MochiAssetResponse? Route { get; init; }
    public MochiAssetResponse? Trk { get; init; }
    public MochiAssetResponse? Entities { get; init; }
    public MochiAssetResponse? GameObjects { get; init; }
    public MochiAssetResponse? RideModifiers { get; init; }
    public MochiAssetResponse? TimedPathSegments { get; init; }
}

public sealed record MochiSpinnerTriggerResponse
{
    public long SegmentHash { get; init; }
    public string? WheelId { get; init; }
}

public sealed record MochiSunPositionResponse
{
    public double Azimuth { get; init; }
    public double Elevation { get; init; }
}

public sealed record MochiAssetResponse
{
    public string? Key { get; init; }
    public long Crc { get; init; }
    public long Size { get; init; }
}

public sealed record MochiResolutionResponse
{
    public string? Resolution { get; init; }
    public double Fps { get; init; }
    public int SegmentCount { get; init; }
    public int FrameCount { get; init; }
    public IReadOnlyList<int> SegmentFrameCounts { get; init; } = [];
    public IReadOnlyList<MochiVideoSegmentResponse> Segments { get; init; } = [];
}

public sealed record MochiVideoSegmentResponse
{
    public int Index { get; init; }
    public MochiAssetResponse? H264 { get; init; }
    public MochiAssetResponse? Zvc { get; init; }
}
