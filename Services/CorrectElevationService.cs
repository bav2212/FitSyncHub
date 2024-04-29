using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StravaWebhooksAzureFunctions.Data.Entities;
using StravaWebhooksAzureFunctions.HttpClients.Interfaces;
using StravaWebhooksAzureFunctions.Options;

namespace StravaWebhooksAzureFunctions.Services;

public class CorrectElevationService
{
    private readonly IStravaCookieAuthHttpClient _authService;
    private readonly IStravaCookieHttpClient _stravaCookieHttpClient;
    private readonly Container _container;
    private readonly ILogger<CorrectElevationService> _logger;
    private readonly StravaOptions _stravaOptions;

    public CorrectElevationService(
        IStravaCookieAuthHttpClient authService,
        IStravaCookieHttpClient stravaCookieHttpClient,
        CosmosClient cosmosClient,
        IOptions<StravaOptions> options,
        ILogger<CorrectElevationService> logger)
    {
        _authService = authService;
        _stravaCookieHttpClient = stravaCookieHttpClient;
        _container = cosmosClient.GetDatabase("strava").GetContainer("SummaryActivity");
        _stravaOptions = options.Value;
        _logger = logger;
    }

    public async Task CorrectElevation(
        DateOnly before,
        DateOnly after,
        CancellationToken cancellationToken)
    {
        var activities = await GetRidesForCorrection(before, after, cancellationToken);

        foreach (var activity in activities)
        {
            var activityId = long.Parse(activity.id);
            await CorrectElevation(activityId, cancellationToken);
        }
    }

    public async Task CorrectElevation(
        long activityId,
        CancellationToken cancellationToken)
    {
        var userName = _stravaOptions.Credentials.Username;
        var password = _stravaOptions.Credentials.Password;

        var authResponse = await _authService.Login(userName, password, cancellationToken);
        if (!authResponse.Success)
        {
            _logger.LogError("Failed login to Strava");
            return;
        }

        await _stravaCookieHttpClient.CorrectElevationIfNeeded(
            activityId,
            authResponse.Cookies,
            authResponse.AuthenticityToken,
            cancellationToken);
    }

    private async Task<List<SummaryActivityData>> GetRidesForCorrection(
        DateOnly before,
        DateOnly after,
        CancellationToken cancellationToken)
    {
        var result = new List<SummaryActivityData>();

        // Get LINQ IQueryable object
        var queryable = _container.GetItemLinqQueryable<SummaryActivityData>();

        // Construct LINQ query
        var matches = queryable
            .Where(x => x.StartDate > after.ToDateTime(TimeOnly.MinValue) && x.StartDate < before.ToDateTime(TimeOnly.MaxValue))
            .Where(x => x.Type == Constants.StravaActivityType.Ride);
        var linqFeed = matches.ToFeedIterator();

        // Iterate query result pages
        while (linqFeed.HasMoreResults)
        {
            var response = await linqFeed.ReadNextAsync(cancellationToken);

            // Iterate query results
            foreach (var item in response)
            {
                result.Add(item);
            }
        }

        return result;
    }
}
