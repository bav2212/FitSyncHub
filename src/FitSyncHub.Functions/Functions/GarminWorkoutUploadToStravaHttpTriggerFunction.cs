using System.IO.Compression;
using System.Text;
using FitSyncHub.Common.Models;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Common;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using DateTime = System.DateTime;

namespace FitSyncHub.Functions.Functions;

public class GarminWorkoutUploadToStravaHttpTriggerFunction
{
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly IStravaHttpClient _stravaHttpClient;
    private readonly ILogger<GarminWorkoutUploadToStravaHttpTriggerFunction> _logger;

    public GarminWorkoutUploadToStravaHttpTriggerFunction(
        GarminConnectHttpClient garminConnectHttpClient,
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        IStravaHttpClient stravaHttpClient,
        ILogger<GarminWorkoutUploadToStravaHttpTriggerFunction> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _stravaHttpClient = stravaHttpClient;
        _logger = logger;
    }

    [Function(nameof(GarminWorkoutUploadToStravaHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "garmin-workout-upload-to-strava")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        string? countQueryParameter = req.Query["count"];
        string? dateQueryParameter = req.Query["date"];

        if (countQueryParameter is null)
        {
            _logger.LogInformation("wrong request");
            return new BadRequestObjectResult("wrong request");
        }

        if (!int.TryParse(countQueryParameter, out var count))
        {
            _logger.LogInformation("Count has wrong format");
            return new BadRequestObjectResult("Count has wrong format");
        }

        if (count == 0)
        {
            _logger.LogInformation("Count should be more than 0");
            return new BadRequestObjectResult("Count should be more than 0");
        }

        if (count > 10)
        {
            _logger.LogInformation("Can't parse more that 10 activities");
            return new BadRequestObjectResult("Can't parse more that 10 activities");
        }

        if (!DateOnly.TryParse(dateQueryParameter, out var date))
        {
            date = DateOnly.FromDateTime(DateTime.Today);
        }

        var garminWorkoutActivities = await _garminConnectHttpClient.GetActivitiesByDate(
            new DateTime(date, TimeOnly.MinValue),
            new DateTime(date, TimeOnly.MaxValue),
            "fitness_equipment",
            cancellationToken);

        if (garminWorkoutActivities.Count != count)
        {
            _logger.LogInformation("Found {ActivitiesCount} garmin activities, but specified {ParsedCount} in request", garminWorkoutActivities.Count, count);
            return new BadRequestObjectResult($"Found {garminWorkoutActivities.Count} garminactivities, but specified {count} in request");
        }

        var intervalsIcuEvents = await _intervalsIcuHttpClient.ListEvents(new(date, date) { Category = [EventCategory.Workout] }, cancellationToken);
        intervalsIcuEvents = [.. intervalsIcuEvents.Where(x => x.Type == "WeightTraining")];

        if (intervalsIcuEvents.Count != count)
        {
            _logger.LogInformation("Found {ActivitiesCount} intervals.icu events, but specified {ParsedCount} in request", intervalsIcuEvents.Count, count);
        }

        var tuples = garminWorkoutActivities.Select((garminWorkoutActivity, index) =>
        {
            var intervalsIcuPlannedEvent = intervalsIcuEvents.ElementAtOrDefault(index);
            return (garminWorkoutActivity, intervalsIcuPlannedEvent);
        });

        var sb = new StringBuilder();
        foreach (var (garminWorkoutActivity, intervalsIcuPlannedEvent) in tuples)
        {
            var garminActivityId = garminWorkoutActivity.ActivityId;
            var garminActivityName = garminWorkoutActivity.ActivityName;
            _logger.LogInformation("Processing activity {ActivityId} with name {ActivityName}", garminActivityId, garminActivityName);

            var activityFileStream = await _garminConnectHttpClient.DownloadActivityFile(garminActivityId, cancellationToken);
            _logger.LogInformation("Downloaded activity file {ActivityId}", garminActivityId);

            using var zip = new ZipArchive(activityFileStream, ZipArchiveMode.Read);
            if (zip.Entries.Count != 1)
            {
                throw new Exception($"Expected 1 entry in zip, but found {zip.Entries.Count}");
            }

            var zipEntry = zip.Entries[0];

            await using var stream = zipEntry.Open();
            await using var memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);

            var uploadFileModel = new FileModel
            {
                Name = zipEntry.Name,
                Bytes = memoryStream.ToArray(),
            };
            _logger.LogInformation("Read {Size} bytes from file {EntryName}", memoryStream.Length, zipEntry.Name);

            // prefer to use intervals.icu event name, because user can rename garmin activity to something like "Treadmill" or "Elliptical"
            // but intervals.icu can be null if it was not planned
            var activityName = intervalsIcuPlannedEvent?.Name ?? garminActivityName;

            var uploadToStravaResult = await UploadToStrava(garminActivityId, activityName, uploadFileModel, cancellationToken);
            if (!uploadToStravaResult.Success)
            {
                sb.AppendLine(uploadToStravaResult.Message);
                continue;
            }
            else
            {
                sb.AppendLine($"Activity '{activityName}' uploaded to Strava");
            }

            var uploadToIntervalscuResult =
                await UploadToIntervalsIcu(intervalsIcuPlannedEvent, garminActivityId, activityName, uploadFileModel, cancellationToken);
            if (!uploadToIntervalscuResult.Success)
            {
                sb.AppendLine(uploadToIntervalscuResult.Message);
                continue;
            }
            else
            {
                sb.AppendLine($"Activity '{activityName}' uploaded to Intervals.icu");
            }
        }

        return new OkObjectResult(sb.ToString());
    }

    private async Task<UploadResult> UploadToStrava(
        long activityId,
        string activityName,
        FileModel uploadFileModel,
        CancellationToken cancellationToken)
    {
        var uploadModel = new StartUploadActivityRequest
        {
            Name = activityName,
            Commute = false,
            Trainer = false,
            DataType = UploadActivityDataType.Fit,
            ExternalId = activityId.ToString(),
        };

        _logger.LogInformation("Uploading activity {ActivityId} with name {ActivityName} to Strava", activityId, activityName);
        var uploadStartResponse = await _stravaHttpClient.UploadStart(uploadFileModel, uploadModel, cancellationToken);
        _logger.LogInformation("Upload started with id {UploadId}", uploadStartResponse.Id);

        var uploadResponse = await _stravaHttpClient.GetUpload(uploadStartResponse.Id, cancellationToken);
        _logger.LogInformation("Upload to strava completed with status: {Status}, Error: {Error}, ActivityId: {ActivityId}",
            uploadResponse.Status,
            uploadResponse.Error,
            uploadResponse.ActivityId);

        if (uploadResponse.Error is not null)
        {
            _logger.LogError("Error from strava: {Error}", uploadResponse.Error);
            return new UploadResult
            {
                Success = false,
                Message = $"Activity '{activityName}', error from strava: {uploadResponse.Error}",
            };
        }

        if (!uploadResponse.ActivityId.HasValue)
        {
            _logger.LogError("Strava activityId is null after upload");
            return new UploadResult
            {
                Success = false,
                Message = $"Activity '{activityName}', activityId is null after upload",
            };
        }

        // hope Strava has some mapping for Garmin activities. Maybe will need this later. 
        // For now I need Garmin `Strength` workout to be uploaded as `WeightTraining` workout to Strava
        //var savedActivity = await _stravaHttpClient.GetActivity(uploadResponse.ActivityId.Value, cancellationToken);
        //if (savedActivity.SportType != SportType.WeightTraining)
        //{
        //    await _stravaHttpClient.UpdateActivity(savedActivity.Id!.Value, new UpdatableActivityRequest
        //    {
        //        SportType = SportType.WeightTraining,
        //    }, cancellationToken);
        //    _logger.LogInformation("Updated sport type from {OldSportType} to Workout", savedActivity.SportType);
        //}

        return UploadResult.Succeed;
    }

    private async Task<UploadResult> UploadToIntervalsIcu(EventResponse? intervalsIcuPlannedEvent,
       long activityId,
       string activityName,
       FileModel uploadFileModel,
       CancellationToken cancellationToken)
    {
        var intervalsIcuCreatedActivity = await _intervalsIcuHttpClient.CreateActivity(uploadFileModel, new CreateActivityRequest
        {
            Name = activityName,
            PairedEventId = intervalsIcuPlannedEvent?.Id,
            Description = $"Garmin Connect activityId: {activityId}",
            ExternalId = activityId.ToString(),
        }, cancellationToken);

        _logger.LogInformation("Activity {ActivityId} uploaded to intervals.icu with id {IntervalsIcuActivityId}", activityId, intervalsIcuCreatedActivity.Id);
        return UploadResult.Succeed;
    }

    private record UploadResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = null!;

        public static UploadResult Succeed => new() { Success = true, Message = string.Empty };
    }
}
