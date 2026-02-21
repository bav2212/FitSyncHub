using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace FitSyncHub.Functions.Functions;

public class YoutubeRedirectToLiveChatHttpTriggerFunction
{
    private readonly ILogger<YoutubeRedirectToLiveChatHttpTriggerFunction> _logger;

    public YoutubeRedirectToLiveChatHttpTriggerFunction(
        ILogger<YoutubeRedirectToLiveChatHttpTriggerFunction> logger)
    {
        _logger = logger;
    }

    [Function(nameof(YoutubeRedirectToLiveChatHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "youtube-live-chat")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var apiKey = Environment.GetEnvironmentVariable("Youtube:ApiKey") ?? throw new InvalidOperationException("YouTube ApiKey is not set in environment variables");
        var channelId = Environment.GetEnvironmentVariable("Youtube:ChannelId") ?? throw new InvalidOperationException("YouTube ChannelId is not set in environment variables");

        var youTubeLiveHelper = new YouTubeLiveHelper(apiKey, channelId);
        var videoId = await youTubeLiveHelper.GetLiveVideoId();

        videoId ??= await youTubeLiveHelper.GetUpcomingVideoId();

        if (videoId == null)
        {
            return new NotFoundObjectResult("No live or upcoming video found for the specified channel.");
        }

        var videLiveChatUrl = $"https://www.youtube.com/live_chat?is_popout=1&v={videoId}";
        return new RedirectResult(videLiveChatUrl);
    }


    public class YouTubeLiveHelper
    {
        private readonly YouTubeService _youtubeService;
        private readonly string _channelId;

        public YouTubeLiveHelper(string apiKey, string channelId)
        {
            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
            });

            _channelId = channelId;
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
}
