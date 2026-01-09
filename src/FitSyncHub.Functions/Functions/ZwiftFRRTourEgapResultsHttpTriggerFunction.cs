using FitSyncHub.Zwift.HttpClients.Abstractions;
using FitSyncHub.Zwift.Models.FRR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public sealed class ZwiftFRRTourEGapResultsHttpTriggerFunction
{
    private readonly IFlammeRougeRacingHttpClient _flammeRougeRacingHttpClient;
    private readonly ILogger<ZwiftFRRTourEGapResultsHttpTriggerFunction> _logger;

    public ZwiftFRRTourEGapResultsHttpTriggerFunction(
        IFlammeRougeRacingHttpClient flammeRougeRacingHttpClient,
        ILogger<ZwiftFRRTourEGapResultsHttpTriggerFunction> logger)
    {
        _flammeRougeRacingHttpClient = flammeRougeRacingHttpClient;
        _logger = logger;
    }

#if DEBUG
    [Function(nameof(ZwiftFRRTourEGapResultsHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-frr-tour-results")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var category = req.Query["category"];
        if (string.IsNullOrWhiteSpace(category) ||
            !Enum.TryParse<FlammeRougeRacingCategory>(category, ignoreCase: true, out var parsedFRRCategory))
        {
            return new BadRequestObjectResult($"Specify params: {nameof(category)}");
        }

        var stage = req.Query["stage"];
        if (!int.TryParse(stage, out var parserStageNumber) || parserStageNumber <= 0)
        {
            return new BadRequestObjectResult($"Specify correct stage number param: {nameof(stage)}");
        }

        var type = req.Query["type"];
        if (string.IsNullOrWhiteSpace(type) ||
            !Enum.TryParse<FlammeRougeRacingTourResultType>(type, ignoreCase: true, out var parsedResultsType))
        {
            var availableValues = string.Join(", ", Enum.GetNames<FlammeRougeRacingTourResultType>()
                .Select(x => x.ToLowerInvariant()));

            return new BadRequestObjectResult($"Specify params: {nameof(type)}. Available values: {availableValues}");
        }

        if (parsedResultsType == FlammeRougeRacingTourResultType.GC)
        {
            var eGapResult = await _flammeRougeRacingHttpClient.GetYellowJerseyStandings(
                parsedFRRCategory, parserStageNumber, cancellationToken);
            return new OkObjectResult(eGapResult);
        }

        if (parsedResultsType == FlammeRougeRacingTourResultType.PolkaDot)
        {
            var eGapResult = await _flammeRougeRacingHttpClient.GetPolkaDotStandings(
                    parsedFRRCategory, parserStageNumber, cancellationToken);
            return new OkObjectResult(eGapResult);
        }

        if (parsedResultsType == FlammeRougeRacingTourResultType.Green)
        {
            var eGapResult = await _flammeRougeRacingHttpClient.GetGreenJerseyStandings(
                parsedFRRCategory, parserStageNumber, cancellationToken);
            return new OkObjectResult(eGapResult);
        }

        throw new NotImplementedException();
    }
}

public enum FlammeRougeRacingTourResultType
{
    GC,
    PolkaDot,
    Green
}
