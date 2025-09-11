using FitSyncHub.Xert.Models.Responses;

namespace FitSyncHub.Xert.Abstractions;

public interface IXertHttpClient
{
    Task<TrainingInfoResponse> GetTrainingInfo(XertWorkoutFormat format, CancellationToken cancellationToken);
    Task<string> GetDownloadWorkout(string downloadUrl, CancellationToken cancellationToken);
}
