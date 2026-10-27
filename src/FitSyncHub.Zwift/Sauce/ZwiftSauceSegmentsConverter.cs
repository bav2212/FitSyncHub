using System.Globalization;
using FitSyncHub.Zwift.Xml.Models;
using FitSyncHub.Zwift.Xml.Models.Entities;

namespace FitSyncHub.Zwift.Sauce;

public class ZwiftSauceSegmentsConverter
{
    public static ZwiftSauceSegment[] Convert(
        uint worldId,
        ZwiftXmlObjectEntitiesRoot zwiftEntities)
    {
        return CreateSegments(worldId, zwiftEntities.World.Entities.TimingArches).ToArray();
    }

    private static IEnumerable<ZwiftSauceSegment> CreateSegments(
        uint worldId,
        IReadOnlyList<ZwiftXmlObjectEntitiesTimingArchElement> timingArches)
    {
        foreach (var timingArch in timingArches)
        {
            if (timingArch.ShowLeaderboardSpecified && !timingArch.ShowLeaderboard)
            {
                continue;
            }

            if (IsSegmentDirectionAvailable(timingArch, reverse: false) &&
                (!timingArch.EnableForwardSpecified || timingArch.EnableForward))
            {
                yield return CreateSegment(timingArch, worldId, reverse: false);
            }

            if (IsSegmentDirectionAvailable(timingArch, reverse: true) &&
                (!timingArch.EnableBackwardSpecified || timingArch.EnableBackward))
            {
                yield return CreateSegment(timingArch, worldId, reverse: true);
            }
        }
    }

    private static bool IsSegmentDirectionAvailable(
        ZwiftXmlObjectEntitiesTimingArchElement timingArch,
        bool reverse)
    {
        return (reverse
                ? timingArch.OverrideNetworkHashReverseSpecified &&
                    timingArch.ArchSegmentDistanceReverseSpecified &&
                    timingArch.StartLineSplineTimeReverseSpecified
                : timingArch.OverrideNetworkHashForwardSpecified &&
                    timingArch.ArchSegmentDistanceForwardSpecified &&
                    timingArch.StartLineSplineTimeForwardSpecified) &&
            timingArch.RoadTimeSpecified &&
            timingArch.ArchColorSpecified;
    }

    private static ZwiftSauceSegment CreateSegment(
        ZwiftXmlObjectEntitiesTimingArchElement timingArch,
        uint worldId,
        bool reverse)
    {
        var id = reverse ? timingArch.OverrideNetworkHashReverse : timingArch.OverrideNetworkHashForward;
        var name = reverse
            ? timingArch.ArchFriendlyNameReverse ?? timingArch.ArchFriendlyName ?? timingArch.Name
            : timingArch.ArchFriendlyName ?? timingArch.Name;

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidDataException("Timing arch is missing its display name.");
        }

        if (reverse &&
            (!timingArch.EnableForwardSpecified || timingArch.EnableForward) &&
            string.Equals(name, timingArch.ArchFriendlyName, StringComparison.Ordinal) &&
            !name.Contains("Reverse", StringComparison.Ordinal))
        {
            name += " Reverse";
        }

        var distance = reverse
            ? timingArch.ArchSegmentDistanceReverse
            : timingArch.ArchSegmentDistanceForward;
        var roadStart = reverse
            ? timingArch.StartLineSplineTimeReverse
            : timingArch.StartLineSplineTimeForward;

        return new ZwiftSauceSegment
        {
            ArchId = timingArch.ArchIdSpecified ? timingArch.ArchId : null,
            Color = $"#{timingArch.ArchColor.ToString("x8", CultureInfo.InvariantCulture)}",
            Distance = Math.Round(distance * 1000, 3),
            GivesPowerUp = !timingArch.GivesPowerUpSpecified || timingArch.GivesPowerUp,
            Id = id.ToString(CultureInfo.InvariantCulture),
            Name = name,
            Reverse = reverse,
            RoadFinish = timingArch.RoadTime,
            RoadId = timingArch.RoadId,
            RoadStart = roadStart,
            WorldId = worldId
        };
    }
}
