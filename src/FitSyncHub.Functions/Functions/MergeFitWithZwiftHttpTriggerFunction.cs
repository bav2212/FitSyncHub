using FitSyncHub.Common.Fit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class MergeFitWithZwiftHttpTriggerFunction
{
    private readonly FitFileDecoder _decoder;
    private readonly FitFileEncoder _encoder;

    public MergeFitWithZwiftHttpTriggerFunction(
        FitFileDecoder decoder,
        FitFileEncoder encoder)
    {
        _decoder = decoder;
        _encoder = encoder;
    }

#if DEBUG
    [Function(nameof(MergeFitWithZwiftHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "merge-fit-with-zwift")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? fitPath = req.Query["fitPath"];
        string? zwiftFitPath = req.Query["zwiftFitPath"];
        string? saveToPath = req.Query["saveToPath"];

        if (string.IsNullOrWhiteSpace(fitPath)
            || string.IsNullOrWhiteSpace(zwiftFitPath)
            || string.IsNullOrWhiteSpace(saveToPath))
        {
            return new BadRequestObjectResult("wrong request");
        }

        var fitFileMessages = _decoder.Decode(fitPath);
        var zwiftFitMessages = _decoder.Decode(zwiftFitPath);

        var mergedFitFile = new ZwiftFitFileMerger(fitFileMessages, zwiftFitMessages);
        await using var memoryStream = new MemoryStream();
        _encoder.Encode(memoryStream, mergedFitFile);

        File.WriteAllBytes(saveToPath, memoryStream.ToArray());

        return new OkObjectResult("Success");
    }
}
