using System.Globalization;
using FitSyncHub.Zwift.Xml.Models;
using FitSyncHub.Zwift.Xml.Models.Route;

namespace FitSyncHub.Zwift.Sauce;

public class ZwiftSauceRouteConverter
{
    public static ZwiftSauceRoute Convert(ZwiftXmlObjectRouteRoot zwiftInGameRoot, uint worldId)
    {

        var route = zwiftInGameRoot.Route;
        var homedata = zwiftInGameRoot.Homedata;

        var zwiftSauceRoute = new ZwiftSauceRoute
        {
            ApprovedForProgressMinimap = route.ApprovedForProgressMinimap,
            AscentBetweenFirstLastLrCPsInMeters = route.AscentBetweenFirstLastLrCPsInMeters,
            AscentInMeters = route.AscentInMeters,
            BikeRec = homedata?.BikeRec,
            BikeType = homedata?.BikeType,
            BlockedForClubs = route.BlockedForClubs,
            BlockedForMeetups = route.BlockedForMeetups,
            BlockedForTimeTrial = route.BlockedForTimeTrial,
            CheckPointCount = route.CheckPointCount,
            DefaultLeadinAscentInMeters = route.DefaultLeadinAscentInMeters,
            DefaultLeadinDistanceInMeters = route.DefaultLeadinDistanceInMeters,
            Difficulty = homedata?.Difficulty,
            DistanceBetweenFirstLastLrCPsInMeters = route.DistanceBetweenFirstLastLrCPsInMeters,
            DistanceInMeters = route.DistanceInMeters,
            Duration = homedata?.Duration,
            EventOnly = route.EventOnly,
            EventPaddocks = route.EventPaddocksRaw,
            ExcludeFromGameDictionary = route.ExcludeFromGameDictionary,
            FreeRideLeadinAscentInMeters = route.FreeRideLeadinAscentInMeters,
            FreeRideLeadinDistanceInMeters = route.FreeRideLeadinDistanceInMeters,
            GameDictName = route.GameDictName,
            HasMatchingLeaderboardSegment = route.HasMatchingLeaderboardSegment,
            HasPortalRoad = route.HasPortalRoad,
            Id = route.NameHash,
            LeadinAscentInMeters = route.LeadInAscentInMeters,
            LeadinDistanceInMeters = route.LeadInDistanceInMeters,
            LevelLocked = route.LevelLocked,
            Manifest = InitManifest(zwiftInGameRoot),
            MeetupLeadinAscentInMeters = route.MeetupLeadinAscentInMeters,
            MeetupLeadinDistanceInMeters = route.MeetupLeadinDistanceInMeters,
            Name = route.Name,
            NormalDecisionCount = route.NormalDecisionCount,
            PrivateSpawnArea = zwiftInGameRoot.PrivateSpawnArea is { } privateSpawnArea
                ? new ZwiftSauceRoutePrivateSpawnArea
                {
                    Endtime = privateSpawnArea.EndTime,
                    Forward = privateSpawnArea.Forward,
                    Road = privateSpawnArea.Road,
                    Starttime = privateSpawnArea.StartTime,
                }
                : null
            ,
            PublishedOn = homedata?.PublishedOn is { } ? DateTime.Parse(homedata.PublishedOn) : null,
            RouteEndsUnderTimingArch = route.RouteEndsUnderTimingArch,
            RouteName = route.RouteName,
            RowRec = homedata?.RowRec,
            RunRec = homedata?.RunRec,
            ScentBetweenFirstLastLrCPsInMeters = route.ScentBetweenFirstLastLrCPsInMeters,
            ShowProgressInHUD = route.ShowProgressInHUD,
            SpawnArea = zwiftInGameRoot.SpawnArea is { } spawnArea
                ? new ZwiftSauceRouteSpawnArea
                {
                    Endtime = spawnArea.EndTime,
                    Forward = spawnArea.Forward,
                    Road = spawnArea.Road,
                    Starttime = spawnArea.StartTime,
                }
                : null,
            SpeedScaledAscentInMeters = homedata?.SpeedScaledAscentInMeters,
            SpeedScaledDistanceInMeters = homedata?.SpeedScaledDistanceInMeters,
            SpeedScaledFreeRideLeadinAscentInMeters = homedata?.SpeedScaledFreeRideLeadinAscentInMeters,
            SpeedScaledFreeRideLeadinDistanceInMeters = homedata?.SpeedScaledFreeRideLeadinDistanceInMeters,
            SpeedScaledLeadinAscentInMeters = homedata?.SpeedScaledLeadinAscentInMeters,
            SpeedScaledLeadinDistanceInMeters = homedata?.SpeedScaledLeadinDistanceInMeters,
            SpeedScaledMeetupLeadinAscentInMeters = homedata?.SpeedScaledMeetupLeadinAscentInMeters,
            SpeedScaledMeetupLeadinDistanceInMeters = homedata?.SpeedScaledMeetupLeadinDistanceInMeters,
            SportType = route.SportType,
            SupportedLaps = route.SupportedLaps,
            SupportsTimeTrialMode = route.SupportsTimeTrialMode,
            UseAlternateEventRamp = route.UseAlternateEventRamp != 0,
            WorkoutRec = homedata?.WorkoutRec,
            WorldId = worldId,
            Xp = homedata?.Xp,
            ZwiftEventOnly = route.ZwiftEventOnly,
            CourseId = worldId switch
            {
                1 => 6,
                2 => 2,
                3 => 7,
                4 => 8,
                5 => 9,
                6 => 10,
                7 => 11,
                8 => 12,
                9 => 13,
                10 => 14,
                11 => 15,
                12 => 16,
                13 => 17,
                _ => throw new NotImplementedException()
            },
            Arches = InitArches(zwiftInGameRoot),
            Segments = InitSegments(zwiftInGameRoot)
        };

        return zwiftSauceRoute;
    }


    private static ZwiftSauceRouteSegment[] InitSegments(ZwiftXmlObjectRouteRoot zwiftInGameRoot)
    {
        return [];

        var routeDistance = zwiftInGameRoot.Route.DistanceInMeters;
        return
        [
            .. (zwiftInGameRoot.SegmentData?.Entries ?? []).Select(entry =>
            {
                var startOffset = entry.PercentStart * routeDistance;
                var endOffset = entry.PercentEnd * routeDistance;

                return new ZwiftSauceRouteSegment
                {
                    Distance = endOffset - startOffset,
                    Id = unchecked((ulong)entry.Hash).ToString(CultureInfo.InvariantCulture),
                    LeadinOnly = startOffset < 0 ? true : null,
                    Offset = startOffset
                };
            })
        ];
    }

    private static ZwiftSauceRouteArch[] InitArches(ZwiftXmlObjectRouteRoot zwiftInGameRoot)
    {
        return [];

        var segments = InitSegments(zwiftInGameRoot);
        if (segments.Length == 0)
        {
            return [];
        }

        var routeSegments = segments.Where(segment => segment.LeadinOnly != true).ToArray();
        if (routeSegments.Length == 0)
        {
            routeSegments = segments;
        }

        var startSegment = routeSegments[0];
        var endSegment = routeSegments[^1];

        return
        [
            new ZwiftSauceRouteArch
            {
                GivesPowerUp = true,
                LeadinOnly = null,
                Offset = 0,
                SegmentId = startSegment.Id
            },
            new ZwiftSauceRouteArch
            {
                GivesPowerUp = true,
                LeadinOnly = null,
                Offset = endSegment.Offset + endSegment.Distance,
                SegmentId = endSegment.Id
            }
        ];
    }

    private static ZwiftSauceRouteManifest[] InitManifest(ZwiftXmlObjectRouteRoot zwiftInGameRoot)
    {
        var leadInGroups = GroupEntries(
            zwiftInGameRoot.LeadInHighResCheckpoint?.Entries ?? [],
            splitOnTimeWrap: true);
        var routeEntries = zwiftInGameRoot.HighResCheckpoint?.Entries ?? [];
        if (zwiftInGameRoot.SpawnArea is { } spawnArea
            && routeEntries.Count > 0
            && routeEntries[0].Road == spawnArea.Road
            && routeEntries[0].Dir == spawnArea.Forward)
        {
            var spawnStart = Math.Min(spawnArea.StartTime, spawnArea.EndTime) - 0.00002;
            var spawnEnd = Math.Max(spawnArea.StartTime, spawnArea.EndTime) + 0.00002;
            var firstRouteEntry = routeEntries.FindIndex(entry =>
                entry.Time < spawnStart || entry.Time > spawnEnd);
            if (firstRouteEntry > 1)
            {
                routeEntries = routeEntries[firstRouteEntry..];
            }
        }

        var routeGroups = GroupEntries(routeEntries, splitOnTimeWrap: true);
        var routeManifest = routeGroups
            .Select((group, index) => CreateManifest(
                group.Entries,
                isLeadin: false,
                startsAfterTimeWrap: group.StartsAfterTimeWrap,
                endsAtTimeWrap: index + 1 < routeGroups.Count && routeGroups[index + 1].StartsAfterTimeWrap))
            .ToArray();
        var checkpointIndexesByGroup = routeGroups
            .Select(_ => new List<int>())
            .ToArray();

        var lastMatchedGroupIndex = 0;
        ZwiftXmlObjectRouteEntryElement? previousCheckpoint = null;
        var checkpoints = zwiftInGameRoot.Checkpoint?.Entries ?? [];
        for (var checkpointIndex = 0; checkpointIndex < checkpoints.Count; checkpointIndex++)
        {
            var checkpoint = checkpoints[checkpointIndex];
            var matchedGroupIndex = -1;
            var firstGroupIndex = lastMatchedGroupIndex;
            if (previousCheckpoint is not null
                && previousCheckpoint.Road == checkpoint.Road
                && previousCheckpoint.Dir == checkpoint.Dir
                && (checkpoint.Dir
                    ? checkpoint.Time < previousCheckpoint.Time - 0.01
                    : checkpoint.Time > previousCheckpoint.Time + 0.01))
            {
                firstGroupIndex++;
            }

            for (var groupIndex = firstGroupIndex; groupIndex < routeGroups.Count; groupIndex++)
            {
                var routeGroup = routeGroups[groupIndex];
                var group = routeGroup.Entries;
                var firstEntry = group[0];
                if (firstEntry.Road != checkpoint.Road || firstEntry.Dir != checkpoint.Dir)
                {
                    continue;
                }

                var minimumTime = group.Min(entry => entry.Time);
                var maximumTime = group.Max(entry => entry.Time);
                var reverse = !firstEntry.Dir;
                var endsAtTimeWrap = groupIndex + 1 < routeGroups.Count
                    && routeGroups[groupIndex + 1].StartsAfterTimeWrap;
                if (routeGroup.StartsAfterTimeWrap)
                {
                    if (reverse)
                    {
                        maximumTime = 1;
                    }
                    else
                    {
                        minimumTime = 0;
                    }
                }

                if (endsAtTimeWrap)
                {
                    if (reverse)
                    {
                        minimumTime = 0;
                    }
                    else
                    {
                        maximumTime = 1;
                    }
                }

                if (checkpoint.Time >= minimumTime - 0.001
                    && checkpoint.Time <= maximumTime + 0.001)
                {
                    matchedGroupIndex = groupIndex;
                    break;
                }
            }

            if (matchedGroupIndex < 0)
            {
                var closestTimeDistance = double.MaxValue;
                for (var groupIndex = firstGroupIndex; groupIndex < routeGroups.Count; groupIndex++)
                {
                    var group = routeGroups[groupIndex].Entries;
                    var firstEntry = group[0];
                    if (firstEntry.Road != checkpoint.Road || firstEntry.Dir != checkpoint.Dir)
                    {
                        continue;
                    }

                    var minimumTime = group.Min(entry => entry.Time);
                    var maximumTime = group.Max(entry => entry.Time);
                    var timeDistance = checkpoint.Time < minimumTime
                        ? minimumTime - checkpoint.Time
                        : checkpoint.Time > maximumTime
                            ? checkpoint.Time - maximumTime
                            : 0;
                    if (timeDistance < closestTimeDistance)
                    {
                        closestTimeDistance = timeDistance;
                        matchedGroupIndex = groupIndex;
                    }
                }

                if (closestTimeDistance > 0.01)
                {
                    matchedGroupIndex = -1;
                }
            }

            if (matchedGroupIndex < 0)
            {
                previousCheckpoint = checkpoint;
                continue;
            }

            checkpointIndexesByGroup[matchedGroupIndex].Add(checkpointIndex + 1);
            lastMatchedGroupIndex = matchedGroupIndex;
            previousCheckpoint = checkpoint;
        }

        for (var groupIndex = 0; groupIndex < routeManifest.Length; groupIndex++)
        {
            var checkpointIndexes = checkpointIndexesByGroup[groupIndex];
            if (checkpointIndexes.Count == 0)
            {
                continue;
            }

            routeManifest[groupIndex].Checkpoints =
            [
                checkpointIndexes[0],
                checkpointIndexes[^1]
            ];
        }

        return
        [
            .. leadInGroups.Select((group, index) => CreateManifest(
                group.Entries,
                isLeadin: true,
                startsAfterTimeWrap: group.StartsAfterTimeWrap,
                endsAtTimeWrap: index + 1 < leadInGroups.Count && leadInGroups[index + 1].StartsAfterTimeWrap)),
            .. routeManifest
        ];

        static List<(List<ZwiftXmlObjectRouteEntryElement> Entries, bool StartsAfterTimeWrap)> GroupEntries(
            List<ZwiftXmlObjectRouteEntryElement> entries,
            bool splitOnTimeWrap)
        {
            List<(List<ZwiftXmlObjectRouteEntryElement> Entries, bool StartsAfterTimeWrap)> groups = [];
            foreach (var entry in entries)
            {
                var previousEntry = groups.Count > 0 ? groups[^1].Entries[^1] : null;
                var startsNewGroup = previousEntry is not null
                    && splitOnTimeWrap
                    && previousEntry.Road == entry.Road
                    && previousEntry.Dir == entry.Dir
                    && (entry.Dir
                        ? entry.Time < previousEntry.Time - 0.01
                        : entry.Time > previousEntry.Time + 0.01);
                var startsAfterTimeWrap = startsNewGroup
                    && Math.Abs(previousEntry!.Time - entry.Time) > 0.5;
                if (groups.Count == 0
                    || previousEntry!.Road != entry.Road
                    || previousEntry.Dir != entry.Dir
                    || startsNewGroup)
                {
                    groups.Add(([], startsAfterTimeWrap));
                }

                groups[^1].Entries.Add(entry);
            }

            return groups;
        }

        static ZwiftSauceRouteManifest CreateManifest(
            List<ZwiftXmlObjectRouteEntryElement> entries,
            bool isLeadin,
            bool startsAfterTimeWrap,
            bool endsAtTimeWrap)
        {
            var reverse = !entries[0].Dir;
            var start = entries[0].Time;
            var end = entries[^1].Time;
            if (reverse)
            {
                (start, end) = (end, start);
            }

            if (startsAfterTimeWrap)
            {
                if (reverse)
                {
                    end = 1;
                }
                else
                {
                    start = 0;
                }
            }

            if (endsAtTimeWrap)
            {
                if (reverse)
                {
                    start = 0;
                }
                else
                {
                    end = 1;
                }
            }

            return new ZwiftSauceRouteManifest
            {
                Checkpoints = null,
                End = end,
                Leadin = isLeadin ? true : null,
                Reverse = reverse,
                RoadId = checked((int)entries[0].Road),
                Start = start
            };
        }
    }
}
