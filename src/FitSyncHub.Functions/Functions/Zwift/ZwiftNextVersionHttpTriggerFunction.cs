using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FitSyncHub.Functions.Functions.Zwift;

public sealed class ZwiftNextVersionHttpTriggerFunction
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ZwiftNextVersionHttpTriggerFunction> _logger;

    public ZwiftNextVersionHttpTriggerFunction(
        HttpClient httpClient,
        ILogger<ZwiftNextVersionHttpTriggerFunction> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

#if DEBUG
    [Function(nameof(ZwiftNextVersionHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "zwift-next-version")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        var previousVersionQueryParam = req.Query["previousVersion"];
        if (string.IsNullOrWhiteSpace(previousVersionQueryParam))
        {
            return new BadRequestObjectResult("Specify params: previousVersion");
        }

        if (!int.TryParse(previousVersionQueryParam, out var previousVersion)
            || previousVersion <= 0)
        {
            return new BadRequestObjectResult("previousVersion is not valid long number");
        }

        List<int> existingVersions = [];
        foreach (var chunk in Enumerable.Range(previousVersion + 1, 1000).Chunk(50).ToList())
        {
            var tasks = chunk.Select(async x =>
            {
                var response = await _httpClient.GetAsync($"https://cdn.zwift.com/gameassets/Zwift_Updates_Root/Zwift_ver_cur.{x}.xml", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
#pragma warning disable CA1873 // Avoid potentially expensive logging
                    _logger.LogInformation("Version {Version} exists", x);
                    existingVersions.Add(x);
#pragma warning restore CA1873 // Avoid potentially expensive logging
                }
            })
            .ToList();

            await Task.WhenAll(tasks);
        }

        return new OkObjectResult(existingVersions);
    }
}
