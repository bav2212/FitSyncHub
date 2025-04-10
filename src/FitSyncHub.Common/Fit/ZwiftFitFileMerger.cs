using Dynastream.Fit;

namespace FitSyncHub.Common.Fit;

public class ZwiftFitFileMerger : FitMessages
{
    private readonly FitMessages _fitMessages;
    private readonly FitMessages _zwiftFitMessages;
    private readonly Dictionary<System.DateTime, ZwiftRecordData> _zwiftDataDictionary;

    public ZwiftFitFileMerger(FitMessages fitMessages, FitMessages zwiftFitMessages)
    {
        _fitMessages = fitMessages;
        _zwiftFitMessages = zwiftFitMessages;

        _zwiftDataDictionary = GetZwiftData(_zwiftFitMessages);


        foreach (var record in _fitMessages.RecordMesgs)
        {
            var timestamp = record.GetTimestamp();

            if (!_zwiftDataDictionary.TryGetValue(timestamp.GetDateTime(), out var zwiftData))
            {
                recordMesgs.Add(record);
                continue;
            }

            var newRecord = new RecordMesg(record);
            newRecord.SetAltitude(zwiftData.Altitude);
            newRecord.SetDistance(zwiftData.Distance);
            newRecord.SetPositionLat(zwiftData.Latitude);
            newRecord.SetPositionLong(zwiftData.Longitude);
            newRecord.SetSpeed(zwiftData.Speed);
            recordMesgs.Add(newRecord);
        }

        SetLapMessages();
        SetSessionMessages();
        SetOtherMessages();
    }

    private Dictionary<System.DateTime, ZwiftRecordData> GetZwiftData(FitMessages zwiftFitMessages)
    {
        Dictionary<System.DateTime, ZwiftRecordData> result = [];

        foreach (var recordMessage in zwiftFitMessages.RecordMesgs)
        {
            var altitude = recordMessage.GetAltitude();
            var distance = recordMessage.GetDistance();
            var latitude = recordMessage.GetPositionLat();
            var longitude = recordMessage.GetPositionLong();
            var speed = recordMessage.GetSpeed();

            if (altitude == null
                || distance == null
                || latitude == null
                || longitude == null
                || speed == null)
            {
                continue;
            }

            var zwiftData = new ZwiftRecordData
            {
                Altitude = altitude.Value,
                Distance = distance.Value,
                Latitude = latitude.Value,
                Longitude = longitude.Value,
                Speed = speed.Value
            };

            var timestamp = recordMessage.GetTimestamp();
            result.Add(timestamp.GetDateTime(), zwiftData);
        }

        return result;
    }

    private void SetSessionMessages()
    {
        var session = new SessionMesg();

        var fitSessionMessage = _fitMessages.SessionMesgs.Single();
        var zwiftFitSessionMessage = _zwiftFitMessages.SessionMesgs.Single();

        session.SetStartTime(fitSessionMessage.GetStartTime());

        session.SetTimestamp(fitSessionMessage.GetTimestamp());
        session.SetSport(fitSessionMessage.GetSport());

        session.SetTotalElapsedTime(fitSessionMessage.GetTotalElapsedTime());
        session.SetTotalTimerTime(fitSessionMessage.GetTotalTimerTime());

        session.SetTotalDistance(zwiftFitSessionMessage.GetTotalDistance());
        session.SetTotalAscent(zwiftFitSessionMessage.GetTotalAscent());

        sessionMesgs.Add(session);
    }

    private void SetLapMessages()
    {
        var firstZwiftData = _zwiftDataDictionary.First();
        var lastZwiftData = _zwiftDataDictionary.Last();

        foreach (var lapMesg in _fitMessages.LapMesgs)
        {
            var lapMessage = new LapMesg(lapMesg);

            var startTime = lapMesg.GetStartTime();
            var elapsedTime = lapMesg.GetTotalElapsedTime() ?? throw new ArgumentException("Lap message does not have elapsed time.");

            var endTime = new Dynastream.Fit.DateTime(startTime);
            endTime.Add((uint)elapsedTime);

            var zwiftDataAtLapStart = startTime.GetDateTime() < firstZwiftData.Key
                ? firstZwiftData.Value
                : _zwiftDataDictionary.GetValueOrDefault(startTime.GetDateTime());

            var zwiftDataAtLapEnd = endTime.GetDateTime() > lastZwiftData.Key
                ? lastZwiftData.Value
                : _zwiftDataDictionary.GetValueOrDefault(endTime.GetDateTime());

            if (zwiftDataAtLapStart is { } && zwiftDataAtLapEnd is { })
            {
                var distance = zwiftDataAtLapEnd.Distance - zwiftDataAtLapStart.Distance;
                lapMessage.SetTotalDistance(distance);
            }

            lapMesgs.Add(lapMessage);
        }
    }

    private void SetOtherMessages()
    {
        activityMesgs.AddRange(_fitMessages.ActivityMesgs);
        developerDataIdMesgs.AddRange(_fitMessages.DeveloperDataIdMesgs);
        deviceInfoMesgs.AddRange(_fitMessages.DeviceInfoMesgs);
        deviceSettingsMesgs.AddRange(_fitMessages.DeviceSettingsMesgs);
        eventMesgs.AddRange(_fitMessages.EventMesgs);
        fieldDescriptionMesgs.AddRange(_fitMessages.FieldDescriptionMesgs);
        fileCreatorMesgs.AddRange(_fitMessages.FileCreatorMesgs);
        fileIdMesgs.AddRange(_fitMessages.FileIdMesgs);
        splitMesgs.AddRange(_fitMessages.SplitMesgs);
        splitSummaryMesgs.AddRange(_fitMessages.SplitSummaryMesgs);
        sportMesgs.AddRange(_fitMessages.SportMesgs);
        timeInZoneMesgs.AddRange(_fitMessages.TimeInZoneMesgs);
        timestampCorrelationMesgs.AddRange(_fitMessages.TimestampCorrelationMesgs);
        trainingFileMesgs.AddRange(_fitMessages.TrainingFileMesgs);
        userProfileMesgs.AddRange(_fitMessages.UserProfileMesgs);
        workoutMesgs.AddRange(_fitMessages.WorkoutMesgs);
        workoutStepMesgs.AddRange(_fitMessages.WorkoutStepMesgs);
        zonesTargetMesgs.AddRange(_fitMessages.ZonesTargetMesgs);
    }
}

internal class ZwiftRecordData
{
    public float Altitude { get; init; }
    public float Distance { get; init; }
    public int Latitude { get; init; }
    public int Longitude { get; init; }
    public float Speed { get; init; }
}
