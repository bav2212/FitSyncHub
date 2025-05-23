using FitSyncHub.Common.Applications.Apple;
using FitSyncHub.Common.Extensions;
using FitSyncHub.Common.Fit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace FitSyncHub.Functions.Functions;

public class MergeFitWithAppleWorkoutHttpTriggerFunction
{
    private readonly FitFileDecoder _decoder;
    private readonly FitFileEncoder _encoder;

    public MergeFitWithAppleWorkoutHttpTriggerFunction(
        FitFileDecoder decoder,
        FitFileEncoder encoder)
    {
        _decoder = decoder;
        _encoder = encoder;
    }

#if DEBUG
    [Function(nameof(MergeFitWithAppleWorkoutHttpTriggerFunction))]
#endif
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "merge-fit-with-apple")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        string? fitPath = req.Query["fitPath"];
        string? appleExportPath = req.Query["appleExportPath"];
        string? saveToPath = req.Query["saveToPath"];

        if (string.IsNullOrWhiteSpace(fitPath)
            || string.IsNullOrWhiteSpace(appleExportPath)
            || string.IsNullOrWhiteSpace(saveToPath))
        {
            return new BadRequestObjectResult("wrong request");
        }

        var fitFileMessages = _decoder.Decode(fitPath);

        var activityStartDate = fitFileMessages.RecordMesgs.Select(x => x.GetTimestamp().GetDateTime()).First();

        var heartRateData = AppleHealthExportFileParser.LoadHeartRateData(appleExportPath,
            from: activityStartDate.AddHours(-1));

        foreach (var recordMessage in fitFileMessages.RecordMesgs)
        {
            var timestamp = recordMessage.GetTimestamp().GetDateTime();

            var closestHeartRateData = heartRateData.GetClosestValue(timestamp, (prev, curr) => prev.Ticks - curr.Ticks);

            recordMessage.SetHeartRate((byte)closestHeartRateData.HeartRate);
        }

        await using var memoryStream = new MemoryStream();
        _encoder.Encode(memoryStream, fitFileMessages);

        File.WriteAllBytes(saveToPath, memoryStream.ToArray());

        return new OkObjectResult("Success");
    }
}
