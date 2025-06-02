using System.IO.Compression;
using System.Text;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.Strava.Abstractions;
using FitSyncHub.Strava.Models;
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
    private readonly IStravaHttpClient _stravaHttpClient;
    private readonly ILogger<GarminWorkoutUploadToStravaHttpTriggerFunction> _logger;

    public GarminWorkoutUploadToStravaHttpTriggerFunction(
        GarminConnectHttpClient garminConnectHttpClient,
        IStravaHttpClient stravaHttpClient,
        ILogger<GarminWorkoutUploadToStravaHttpTriggerFunction> logger)
    {
        _garminConnectHttpClient = garminConnectHttpClient;
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

        var activities = await _garminConnectHttpClient.GetActivitiesByDate(
            new DateTime(date, TimeOnly.MinValue),
            new DateTime(date, TimeOnly.MaxValue),
            "fitness_equipment",
            cancellationToken);

        if (activities.Count != count)
        {
            _logger.LogInformation("Found {ActivitiesCount} todays activities, but specified {ParsedCount} in request", activities.Count, count);
            return new BadRequestObjectResult($"Found {activities.Count} todays activities, but specified {count} in request");
        }

        var sb = new StringBuilder();

        foreach (var workoutActivity in activities)
        {
            var activityId = workoutActivity.ActivityId;
            var activityName = workoutActivity.ActivityName;
            _logger.LogInformation("Processing activity {ActivityId} with name {ActivityName}", activityId, activityName);

            var activityFileStream = await _garminConnectHttpClient.DownloadActivityFile(activityId, cancellationToken);
            _logger.LogInformation("Downloaded activity file {ActivityId}", activityId);

            using var zip = new ZipArchive(activityFileStream, ZipArchiveMode.Read);
            if (zip.Entries.Count != 1)
            {
                throw new Exception($"Expected 1 entry in zip, but found {zip.Entries.Count}");
            }

            var zipEntry = zip.Entries[0];

            await using var stream = zipEntry.Open();
            await using var memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);

            var uploadFileModel = new Common.Models.FileModel
            {
                Name = zipEntry.Name,
                Bytes = memoryStream.ToArray(),
            };
            _logger.LogInformation("Read {Size} bytes from file {EntryName}", memoryStream.Length, zipEntry.Name);

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
                sb.AppendLine($"Activity '{activityName}', error from strava: {uploadResponse.Error}");
                continue;
            }

            if (!uploadResponse.ActivityId.HasValue)
            {
                _logger.LogError("Strava activityId is null after upload");
                sb.AppendLine($"Activity '{activityName}', activityId is null after upload");
                continue;
            }

            var savedActivity = await _stravaHttpClient.GetActivity(uploadResponse.ActivityId.Value, cancellationToken);
            if (savedActivity.SportType != SportType.Workout)
            {
                await _stravaHttpClient.UpdateActivity(savedActivity.Id!.Value, new UpdatableActivityRequest
                {
                    SportType = SportType.Workout,
                }, cancellationToken);
                _logger.LogInformation("Updated sport type from {OldSportType} to Workout", savedActivity.SportType);
            }
            sb.AppendLine($"Activity '{activityName}' uploaded");
        }

        return new OkObjectResult(sb.ToString());
    }
}
