using FitSyncHub.Zwift.Sauce;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions.Zwift;

public class ZwiftSauceDataHttpTriggerFunction
{
    private readonly ZwiftSauceService _zwiftSauceService;

    public ZwiftSauceDataHttpTriggerFunction(
        ZwiftSauceService zwiftSauceService)
    {
        _zwiftSauceService = zwiftSauceService;
    }

    [Function(nameof(ZwiftSauceDataHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "sauce-data")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _ = req;

        await _zwiftSauceService.InitializeSauceData(cancellationToken);

        return new OkObjectResult("");
    }
}
