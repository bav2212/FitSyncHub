using System.Xml.Linq;
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

        List<ZwiftNextVersionResponse> results = [];
        foreach (var chunk in Enumerable.Range(previousVersion + 1, 1000).Chunk(50).ToList())
        {
            var tasks = chunk.Select(version => GetZwiftNextVersionTask(version, cancellationToken));
            await foreach (var task in Task.WhenEach(tasks))
            {
                var item = await task;
                if (item is not null)
                {
                    results.Add(item);
                }
            }
        }

        results = [.. results.OrderBy(x => x.Version)];

        return new OkObjectResult(results);
    }

    private async Task<ZwiftNextVersionResponse?> GetZwiftNextVersionTask(int version, CancellationToken cancellationToken)
    {
        var requestUrl = $"https://cdn.zwift.com/gameassets/Zwift_Updates_Root/Zwift_ver_cur.{version}.xml";

        var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var responseContentXml = await response.Content.ReadAsStringAsync(cancellationToken);
        var sversion = XElement.Parse(responseContentXml)
               .Attribute("sversion")
               ?.Value ?? throw new InvalidOperationException("Failed to get sversion");

#pragma warning disable CA1873 // Avoid potentially expensive logging
        _logger.LogInformation("Version {Version} exists, SVersion: {SVersion}, URL: {Url}",
            version,
            sversion,
            requestUrl);
#pragma warning restore CA1873 // Avoid potentially expensive logging

        return new ZwiftNextVersionResponse
        {
            Version = version,
            VersionName = sversion,
            Url = requestUrl
        };
    }

    public sealed class ZwiftNextVersionResponse
    {
        public required int Version { get; init; }
        public required string VersionName { get; init; }
        public required string Url { get; init; }
    }
}
