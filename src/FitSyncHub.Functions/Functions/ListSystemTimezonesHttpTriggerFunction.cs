using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class ListSystemTimezonesHttpTriggerFunction
{
    [Function(nameof(ListSystemTimezonesHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "list-time-zones")] HttpRequest req)
    {
        _ = req;

        return new OkObjectResult(TimeZoneInfo.GetSystemTimeZones());
    }
}
