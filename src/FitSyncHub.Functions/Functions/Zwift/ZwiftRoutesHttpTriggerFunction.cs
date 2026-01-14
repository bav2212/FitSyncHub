using FitSyncHub.Zwift.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions.Zwift;

public class ZwiftRoutesHttpTriggerFunction
{
    private readonly ZwiftRoutesService _zwiftRoutesService;

    public ZwiftRoutesHttpTriggerFunction(
        ZwiftRoutesService zwiftRoutesService)
    {
        _zwiftRoutesService = zwiftRoutesService;
    }

    [Function(nameof(ZwiftRoutesHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-routes")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _ = req;

        var routes = await _zwiftRoutesService.GetRoutesInfo(cancellationToken);
        var orderdRoutes = routes
             .OrderBy(x => x.PublishedOn)
             .ToList();

        return new OkObjectResult(orderdRoutes);
    }
}
