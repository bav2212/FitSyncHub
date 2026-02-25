using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace FitSyncHub.Youtube.Services;

public class YouTubeLiveService
{
    private readonly YouTubeService _youtubeService;

    public YouTubeLiveService(YouTubeService youTubeService)
    {
        _youtubeService = youTubeService;
    }

    public async Task<string?> GetNextUpcomingVideoId(CancellationToken cancellationToken)
    {
        var liveBroadcastsRequest = _youtubeService.LiveBroadcasts.List("snippet");
        liveBroadcastsRequest.BroadcastStatus = LiveBroadcastsResource.ListRequest.BroadcastStatusEnum.Upcoming;

        var response = await GetAllPagesAsync(liveBroadcastsRequest, cancellationToken);

        return response
            .OrderBy(x => x.Snippet.ScheduledStartTimeDateTimeOffset)
            .Select(x => x.Id)
            .FirstOrDefault();
    }

    public async Task<string?> GetLiveVideoId(CancellationToken cancellationToken)
    {
        var liveBroadcastsRequest = _youtubeService.LiveBroadcasts.List("snippet");
        liveBroadcastsRequest.BroadcastStatus = LiveBroadcastsResource.ListRequest.BroadcastStatusEnum.Active;

        var response = await GetAllPagesAsync(liveBroadcastsRequest, cancellationToken);

        return response.Select(x => x.Id).SingleOrDefault();
    }

    private static async Task<List<LiveBroadcast>> GetAllPagesAsync(
        LiveBroadcastsResource.ListRequest liveBroadcastsRequest, CancellationToken cancellationToken)
    {
        string? nextPageToken = null;
        List<LiveBroadcast> result = [];

        do
        {
            liveBroadcastsRequest.PageToken = nextPageToken;

            var response = await liveBroadcastsRequest.ExecuteAsync(cancellationToken);
            result.AddRange(response.Items);

            nextPageToken = response.NextPageToken;
        } while (!string.IsNullOrEmpty(nextPageToken));

        return result;
    }
}
