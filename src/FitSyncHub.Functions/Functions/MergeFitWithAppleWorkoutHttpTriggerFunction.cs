using System.Numerics;
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

            var closestHeartRateData = GetClosestValue(heartRateData, timestamp, (prev, curr) => prev.Ticks - curr.Ticks);

            recordMessage.SetHeartRate((byte)closestHeartRateData.HeartRate);
        }

        await using var memoryStream = new MemoryStream();
        _encoder.Encode(memoryStream, fitFileMessages);

        await File.WriteAllBytesAsync(saveToPath, memoryStream.ToArray(), cancellationToken);

        return new OkObjectResult("Success");
    }

    private static TValue GetClosestValue<TKey, TValue, TDiffResult>(
      SortedList<TKey, TValue> sortedList,
      TKey target,
      Func<TKey, TKey, TDiffResult> diffFunc)
      where TKey : notnull, IComparable<TKey>
      where TDiffResult : INumber<TDiffResult>
    {
        if (sortedList.Count == 0)
        {
            throw new InvalidOperationException("List is empty");
        }

        var keys = sortedList.Keys;
        var left = 0;
        var right = keys.Count - 1;

        while (left <= right)
        {
            var mid = (left + right) / 2;
            var cmp = keys[mid].CompareTo(target);

            if (cmp == 0)
            {
                return sortedList[keys[mid]];
            }
            else if (cmp < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        // Now left is the insertion point
        if (left == 0)
        {
            return sortedList[keys[0]];
        }

        if (left == keys.Count)
        {
            return sortedList[keys[^1]];
        }

        // Compare the two neighbors to find the closest
        var before = keys[left - 1];
        var after = keys[left];

        var diffBefore = TDiffResult.Abs(diffFunc(target, before));
        var diffAfter = TDiffResult.Abs(diffFunc(after, target));

        return diffBefore <= diffAfter ? sortedList[before] : sortedList[after];
    }
}
