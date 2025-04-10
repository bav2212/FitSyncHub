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
        var fitPath = @"C:\Users\bav22\Downloads\18784196497_ACTIVITY.fit";
        var zwiftFitPath = @"C:\Users\bav22\Downloads\Zwift_Pacer_Group_Ride_The_Big_Ring_in_Watopia_with_Maria.fit";

        var fitFileMessages = _decoder.Decode(fitPath);
        var zwiftFitMessages = _decoder.Decode(zwiftFitPath);

        var mergedFitFile = new ZwiftFitFileMerger(fitFileMessages, zwiftFitMessages);
        await using var memoryStream = new MemoryStream();
        _encoder.Encode(memoryStream, mergedFitFile);

        File.WriteAllBytes(@"C:\Users\bav22\Downloads\merged.fit", memoryStream.ToArray());

        return new OkObjectResult("Success");
    }
}
