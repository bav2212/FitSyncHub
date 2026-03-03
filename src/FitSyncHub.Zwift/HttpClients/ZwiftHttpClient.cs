using System.Text.Json;
using FitSyncHub.Zwift.HttpClients.Models.Responses.GameInfo;
using FitSyncHub.Zwift.JsonSerializerContexts;
using FitSyncHub.Zwift.Protobuf;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Zwift.HttpClients;

public sealed partial class ZwiftHttpClient
{
    private readonly HttpClient _httpClientJson;
    private readonly HttpClient _httpClientProto;
    private readonly ILogger<ZwiftHttpClient> _logger;

    public ZwiftHttpClient(IHttpClientFactory httpClientFactory, ILogger<ZwiftHttpClient> logger)
    {
        _httpClientJson = httpClientFactory.CreateClient(Constants.ZwiftHttpClientJson);
        _httpClientProto = httpClientFactory.CreateClient(Constants.ZwiftHttpClientProto);
        _logger = logger;
    }

    public async Task<List<int>> GetAchievements(CancellationToken cancellationToken)
    {
        const string Url = "achievement/loadPlayerAchievements";

        var response = await _httpClientProto.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var achievements = Achievements.Parser.ParseFrom(stream);

        return [.. achievements.Achievements_.Select(x => x.Id)];
    }

    public async Task<ZwiftGameInfoResponse> GetGameInfo(CancellationToken cancellationToken)
    {
        const string Url = "api/game_info";

        var response = await _httpClientJson.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize(content,
            ZwiftHttpClientGameInfoGenerationContext.Default.ZwiftGameInfoResponse)!;
    }

    public async Task<List<PayloadSegmentResult>> GetSegmentResults(
        long playerId,
        int worldId,
        long segmentId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        if (to <= from)
        {
            throw new ArgumentException($"'{nameof(to)}' should be greater than or equal to '{nameof(from)}'");
        }

        var baseQueryParams = new Dictionary<string, StringValues>
        {
            { "world_id", worldId.ToString() },
            { "player_id", playerId.ToString() },
            { "segment_id", segmentId.ToString() },
        };

        List<PayloadSegmentResult> results = [];

        do
        {
            var fromQueryParam = GetMaxFromApiDate(to) > from
                ? GetMaxFromApiDate(to)
                : from;

            var queryParams = new Dictionary<string, StringValues>(baseQueryParams)
            {
                { "from", fromQueryParam.ToString("yyyy-MM-ddTHH:mm:ssZ")  },
                { "to", to.ToString("yyyy-MM-ddTHH:mm:ssZ") }
            };

            var url = QueryHelpers.AddQueryString("api/segment-results", queryParams);

            var response = await _httpClientProto.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var segmentResults = SegmentResults.Parser.ParseFrom(stream);

            results.AddRange(segmentResults.Results);

            to = GetMaxFromApiDate(to);
        }
        while (to >= from);

        return results;

        static DateTime GetMaxFromApiDate(DateTime date) => date.AddDays(-100);
    }
}
