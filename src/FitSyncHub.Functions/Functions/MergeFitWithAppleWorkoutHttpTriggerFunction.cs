using FitSyncHub.Common.Applications.Apple;
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

        if (string.IsNullOrWhiteSpace(fitPath) || string.IsNullOrWhiteSpace(appleExportPath))
        {
            return new BadRequestObjectResult("wrong request");
        }

        var heartRateData = AppleHealthExportFileParser.LoadHeartRateData(appleExportPath);

        heartRateData = [.. heartRateData.Where(x => x.Time > DateTime.UtcNow.AddDays(-1))];

        var fitFileMessages = _decoder.Decode(fitPath);

        foreach (var recordMessage in fitFileMessages.RecordMesgs)
        {
            var timestamp = recordMessage.GetTimestamp().GetDateTime();

            var heartRate = AppleHealthExportFileParser.FindClosestHeartRate(
                heartRateData,
                timestamp);

            recordMessage.SetHeartRate((byte)heartRate);
        }

        await using var memoryStream = new MemoryStream();
        _encoder.Encode(memoryStream, fitFileMessages);

        File.WriteAllBytes(@"C:\Users\bav22\Downloads\merged.fit", memoryStream.ToArray());

        return new OkObjectResult("Success");
    }
}
