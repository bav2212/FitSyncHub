using System.Numerics;
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
        var acc = new SessionMesg();

        foreach (var fitMessages in _fitMessagesList)
        {
            foreach (var sessionMsg in fitMessages.SessionMesgs)
            {
                // cause should take smallest value
                if (acc.GetStartTime() is not { })
                {
                    acc.SetStartTime(sessionMsg.GetStartTime());
                }

                acc.SetTimestamp(sessionMsg.GetTimestamp());
                acc.SetSport(sessionMsg.GetSport());

                acc.SetTotalElapsedTime(MergeFieldValueWith(func => func.GetTotalElapsedTime()));
                acc.SetTotalTimerTime(MergeFieldValueWith(func => func.GetTotalTimerTime()));
                acc.SetTotalDistance(MergeFieldValueWith(func => func.GetTotalDistance()));
                acc.SetTotalAscent(MergeFieldValueWith(func => func.GetTotalAscent()));
                acc.SetMetabolicCalories(MergeFieldValueWith(func => func.GetMetabolicCalories()));

                T? MergeFieldValueWith<T>(Func<SessionMesg, T?> func) where T : struct, INumber<T>
                {
                    var value1 = func(acc);
                    var value2 = func(sessionMsg);

                    return (value1, value2) switch
                    {
                        (null, null) => null,
                        (null, _) => value2,
                        (_, null) => value1,
                        (var v1, var v2) => v1 + v2
                    };
                }
            }
        }

        sessionMesgs.Add(acc);
    }

    private void SetFileIdMessages()
    {
        fileIdMesgs.AddRange(_fitMessagesFirst.FileIdMesgs);
    }
}
