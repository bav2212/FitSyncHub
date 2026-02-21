using FitSyncHub.Youtube.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;


namespace FitSyncHub.Functions.Functions;

public class YoutubeRedirectToLiveChatHttpTriggerFunction
{
    private readonly YouTubeLiveService _youTubeLiveService;
    readonly ILogger<YoutubeRedirectToLiveChatHttpTriggerFunction> _logger;

    public YoutubeRedirectToLiveChatHttpTriggerFunction(
        YouTubeLiveService youTubeLiveService,
        ILogger<YoutubeRedirectToLiveChatHttpTriggerFunction> logger)
    {
        _youTubeLiveService = youTubeLiveService;
        _logger = logger;
    }

    [Function(nameof(YoutubeRedirectToLiveChatHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "youtube-live-chat")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var videoId = await _youTubeLiveService.GetLiveVideoId();
        if (videoId is null)
        {
            _logger.LogInformation("No live video found, checking for upcoming videos.");
        }

        videoId ??= await _youTubeLiveService.GetUpcomingVideoId();
        if (videoId == null)
        {
            _logger.LogInformation("No upcoming video found for the specified channel.");
            return new NotFoundObjectResult("No live or upcoming video found for the specified channel.");
        }

        var videLiveChatUrl = $"https://www.youtube.com/live_chat?is_popout=1&v={videoId}";
        return new RedirectResult(videLiveChatUrl);
    }
}
