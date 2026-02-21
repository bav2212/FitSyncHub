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

    public async Task<string?> GetUpcomingVideoId()
    {
        var searchRequest = _youtubeService.Search.List("snippet");
        searchRequest.ChannelId = _channelId;
        searchRequest.EventType = SearchResource.ListRequest.EventTypeEnum.Upcoming;
        searchRequest.Type = "video";

        var searchResponse = await searchRequest.ExecuteAsync();

        return searchResponse.Items.FirstOrDefault()?.Id.VideoId;
    }

    public async Task<string?> GetLiveVideoId()
    {
        var searchRequest = _youtubeService.Search.List("snippet");
        searchRequest.ChannelId = _channelId;
        searchRequest.EventType = SearchResource.ListRequest.EventTypeEnum.Live;
        searchRequest.Type = "video";

        var searchResponse = await searchRequest.ExecuteAsync();

        return searchResponse.Items.FirstOrDefault()?.Id.VideoId;
    }
}
