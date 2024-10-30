using System.Net;
using FitSyncHub.Functions.Extensions;
using FitSyncHub.IntervalsICU.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions;

public class WhatsOnZwiftToIntervalsICUConverterHttpTriggerFunction
{
    private readonly ZwiftToIntervalsIcuService _zwiftToIntervalsIcuService;

    public WhatsOnZwiftToIntervalsICUConverterHttpTriggerFunction(ZwiftToIntervalsIcuService zwiftToIntervalsIcuService)
    {
        _zwiftToIntervalsIcuService = zwiftToIntervalsIcuService;
    }

    [Function(nameof(WhatsOnZwiftToIntervalsICUConverterHttpTriggerFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "whats-on-zwift-to-intervals")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger<WhatsOnZwiftToIntervalsICUConverterHttpTriggerFunction>();
        logger.LogInformation("C# HTTP trigger function processed a request.");

        var url = req.Query["url"];
        if (url == null)
        {
            return req.CreateBadRequest("wrong request");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return req.CreateBadRequest("wrong url");
        }

        try
        {
            var workout = await _zwiftToIntervalsIcuService.ScrapeAndConvertToIntervalsIcu(uri);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            await response.WriteStringAsync($"{workout.FileInfo.Name}\n");

            var responseContent = string.Join("\n", workout.IntervalsIcuStructure);
            await response.WriteStringAsync(responseContent);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot convert WhatsOnZwift workout to Intervals.ICU workout");
            return req.CreateBadRequest("Cannot convert WhatsOnZwift workout to Intervals.ICU workout");
        }
    }
}
