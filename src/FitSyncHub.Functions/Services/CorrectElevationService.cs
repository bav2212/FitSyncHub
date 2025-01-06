using FitSyncHub.Functions.HttpClients.Interfaces;
using FitSyncHub.Functions.Options;
using FitSyncHub.Functions.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FitSyncHub.Functions.Services;

public class CorrectElevationService
{
    private readonly IStravaCookieAuthHttpClient _authService;
    private readonly IStravaCookieHttpClient _stravaCookieHttpClient;
    private readonly SummaryActivityRepository _summaryActivityRepository;
    private readonly ILogger<CorrectElevationService> _logger;
    private readonly StravaOptions _stravaOptions;

    public CorrectElevationService(
        IStravaCookieAuthHttpClient authService,
        IStravaCookieHttpClient stravaCookieHttpClient,
        SummaryActivityRepository summaryActivityRepository,
        IOptions<StravaOptions> options,
        ILogger<CorrectElevationService> logger)
    {
        _authService = authService;
        _stravaCookieHttpClient = stravaCookieHttpClient;
        _summaryActivityRepository = summaryActivityRepository;
        _stravaOptions = options.Value;
        _logger = logger;
    }

    public async Task<int> CorrectElevation(
        DateOnly before,
        DateOnly after,
        CancellationToken cancellationToken)
    {
        var activities = await _summaryActivityRepository.ReadItems(x =>
            x.StartDate > after.ToDateTime(TimeOnly.MinValue)
                && x.StartDate < before.ToDateTime(TimeOnly.MaxValue)
                && x.Type == Constants.StravaActivityType.Ride, cancellationToken);

        foreach (var activity in activities)
        {
            var activityId = long.Parse(activity.id);
            await CorrectElevation(activityId, cancellationToken);
        }

        return activities.Count;
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

        _logger.LogInformation("Correcting elevation for activity {ActivityId}", activityId);

        await _stravaCookieHttpClient.CorrectElevationIfNeeded(
            activityId,
            authResponse.Cookies,
            authResponse.AuthenticityToken,
            cancellationToken);
    }
}
