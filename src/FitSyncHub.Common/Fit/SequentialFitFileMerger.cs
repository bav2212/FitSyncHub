using Dynastream.Fit;

namespace FitSyncHub.Common.Fit;

public sealed class SequentialFitFileMerger : FitMessages
{
    private readonly List<FitMessages> _fitMessagesList;
    private readonly FitMessages _fitMessagesFirst;

    public SequentialFitFileMerger(params List<FitMessages> fitMessagesParams)
    {
        if (fitMessagesParams.Count < 1)
        {
            throw new ArgumentException("Fit messages params should have at least one element", nameof(fitMessagesParams));
        }

        _fitMessagesList = [.. fitMessagesParams
            .OrderBy(x => x.FileIdMesgs[0].GetTimeCreated().GetDateTime())];
        _fitMessagesFirst = _fitMessagesList[0];

        SetFileIdMessages();
        SetSessionMessages();
        SetActivityMessages();
        SetOtherMessages();
    }

    private void SetOtherMessages()
    {
        foreach (var fitMessages in _fitMessagesList)
        {
            developerDataIdMesgs.AddRange(fitMessages.DeveloperDataIdMesgs);
            fieldDescriptionMesgs.AddRange(fitMessages.FieldDescriptionMesgs);
            eventMesgs.AddRange(fitMessages.EventMesgs);
            deviceInfoMesgs.AddRange(fitMessages.DeviceInfoMesgs);
            sportMesgs.AddRange(fitMessages.SportMesgs);
            workoutMesgs.AddRange(fitMessages.WorkoutMesgs);
            recordMesgs.AddRange(fitMessages.RecordMesgs);
            lapMesgs.AddRange(fitMessages.LapMesgs);
        }
    }

    private void SetActivityMessages()
    {
        var result = new ActivityMesg(_fitMessagesFirst.ActivityMesgs.Single());
        // copy all and reset total timer time
        result.SetTotalTimerTime(0);

        foreach (var fitMessages in _fitMessagesList)
        {
            foreach (var activityMsg in fitMessages.ActivityMesgs)
            {
                result.SetTotalTimerTime(result.GetTotalTimerTime().GetValueOrDefault() +
                    activityMsg.GetTotalTimerTime().GetValueOrDefault());
                result.SetTimestamp(activityMsg.GetTimestamp());
            }
        }

        activityMesgs.Add(result);
    }

    private void SetSessionMessages()
    {
        var session = new SessionMesg();

        foreach (var fitMessages in _fitMessagesList)
        {
            foreach (var sessionMsg in fitMessages.SessionMesgs)
            {
                // cause should take smallest value
                if (session.GetStartTime() is not { })
                {
                    session.SetStartTime(sessionMsg.GetStartTime());
                }

                session.SetTimestamp(sessionMsg.GetTimestamp());
                session.SetSport(sessionMsg.GetSport());

                session.SetTotalElapsedTime(
                    session.GetTotalElapsedTime().GetValueOrDefault() +
                    sessionMsg.GetTotalElapsedTime().GetValueOrDefault());

                session.SetTotalTimerTime(
                    session.GetTotalTimerTime().GetValueOrDefault() +
                    sessionMsg.GetTotalTimerTime().GetValueOrDefault());

                session.SetTotalDistance(
                    session.GetTotalDistance().GetValueOrDefault() +
                    sessionMsg.GetTotalDistance().GetValueOrDefault());

                var totalAscent = session.GetTotalAscent().GetValueOrDefault() +
                    sessionMsg.GetTotalAscent().GetValueOrDefault();
                session.SetTotalAscent((ushort)totalAscent);
            }
        }

        sessionMesgs.Add(session);
    }

    private void SetFileIdMessages()
    {
        fileIdMesgs.AddRange(_fitMessagesFirst.FileIdMesgs);
    }
}
