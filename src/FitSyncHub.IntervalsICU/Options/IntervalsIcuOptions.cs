using Microsoft.Extensions.Options;

namespace FitSyncHub.IntervalsICU.Options;

public sealed record IntervalsIcuOptions : IOptions<IntervalsIcuOptions>
{
    public required string ApiKey { get; set; }
    public required string AthleteId { get; set; }
    public string ApiAddress { get; set; } = "https://intervals.icu/api/v1/";

    IntervalsIcuOptions IOptions<IntervalsIcuOptions>.Value => this;
}
