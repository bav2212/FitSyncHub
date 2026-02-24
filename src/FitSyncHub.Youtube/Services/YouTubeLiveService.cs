using FitSyncHub.Youtube.Options;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Youtube.Services;

public class YouTubeLiveService
{
    private readonly YouTubeService _youtubeService;
    private readonly string _channelId;

    public YouTubeLiveService(YouTubeService youTubeService, IOptions<YoutubeOptions> options)
    {
        _youtubeService = youTubeService;
        _channelId = options.Value.ChannelId;
    }

    public async Task<string?> GetNextUpcomingVideoId(CancellationToken cancellationToken)
    {
        var searchListRequest = _youtubeService.Search.List("snippet");
        searchListRequest.ChannelId = _channelId;
        searchListRequest.EventType = SearchResource.ListRequest.EventTypeEnum.Upcoming;
        searchListRequest.Type = "video";

        var searchResponse = await searchListRequest.ExecuteAsync(cancellationToken);

        var upcomingVideoIds = searchResponse.Items.Select(item => item.Id.VideoId).ToHashSet();
        var videosListRequest = _youtubeService.Videos.List("liveStreamingDetails");
        videosListRequest.Id = new Google.Apis.Util.Repeatable<string>(upcomingVideoIds);

        var videosListResponse = await videosListRequest.ExecuteAsync(cancellationToken);

        return videosListResponse.Items
            .OrderBy(x => x.LiveStreamingDetails.ScheduledStartTimeDateTimeOffset)
            .Select(x => x.Id)
            .FirstOrDefault();
    }

    public async Task<string?> GetLiveVideoId(CancellationToken cancellationToken)
    {
        var searchListRequest = _youtubeService.Search.List("snippet");
        searchListRequest.ChannelId = _channelId;
        searchListRequest.EventType = SearchResource.ListRequest.EventTypeEnum.Live;
        searchListRequest.Type = "video";

        var searchResponse = await searchListRequest.ExecuteAsync(cancellationToken);

        return searchResponse.Items.FirstOrDefault()?.Id.VideoId;
    }
}
