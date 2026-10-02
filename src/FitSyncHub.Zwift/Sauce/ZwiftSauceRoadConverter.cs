using System.Globalization;
using FitSyncHub.Zwift.Xml.Models;
using FitSyncHub.Zwift.Xml.Models.Road;

namespace FitSyncHub.Zwift.Sauce;

public class ZwiftSauceRoadConverter
{
    public static ZwiftSauceRoad[] Convert(
        ZwiftXmlObjectRoadRoot zwiftRoadRoot,
        ZwiftXmlObjectRoadStyleRoot zwiftRoadStyleRoot)
    {
        var zwiftRoadStylesDictionary = zwiftRoadStyleRoot.Roads.Segments
            .Select((x, index) => new { Index = index, Style = x.Style! })
            .ToDictionary(x => x.Index, x => x.Style);

        return [.. zwiftRoadRoot.World.Roads
            .Where(road => road.AllowedSport.GetValueOrDefault() != 0)
            .Select(road => new ZwiftSauceRoad
        {
            DefaultStyle = GetDefaultStyle(road.DefaultStyle, zwiftRoadStylesDictionary),
            Id = road.Id,
            IsAvailable = road.IsAvailable == 1,
            Looped = road.Looped == 1,
            OneWay = road.OneWay == 1,
            Path = [.. road.Entities
                .Where(entity => entity.Type == "ENTITY_TYPE_ROADNODE" && entity.Position is not null)
                .Select(entity => CreatePathPoint(entity))],
            Segments = [],
            SplineType = road.SplineType ?? "CatmullRom",
            Sports = GetSports(road.AllowedSport),
            Styles = GetStyles(road, zwiftRoadStylesDictionary)
        })];
    }

    private static ZwiftSauceRoadStyle[] GetStyles(
        ZwiftXmlObjectRoadWorldRoadElement road,
        Dictionary<int, string> styleMapping)
    {
        var defaultStyleId = road.DefaultStyle;
        var defaultStyle = defaultStyleId.HasValue && styleMapping.TryGetValue(defaultStyleId.Value, out var mappedDefaultStyle)
            ? mappedDefaultStyle
            : null;

        var roadMarkers = road.Entities
            .Where(entity => entity.Type == "ENTITY_TYPE_ROADMARKER" && entity.StyleSpecified && styleMapping.ContainsKey(entity.Style))
            .ToList();

        if (roadMarkers.Count == 0 || (defaultStyleId.HasValue && roadMarkers.All(marker => marker.Style == defaultStyleId.Value)))
        {
            return defaultStyle is not null && defaultStyle != "NORMAL"
                ? [new ZwiftSauceRoadStyle { Start = -1, End = 2, Style = defaultStyle }]
                : [];
        }

        var styleMarkers = roadMarkers
            .Where(marker => marker.Style != defaultStyleId)
            .Select(marker => (
                Start: marker.RoadTime1Specified ? marker.RoadTime1 : -1,
                End: marker.RoadTime2Specified ? marker.RoadTime2 : 2,
                StyleId: marker.Style,
                Style: styleMapping[marker.Style]))
            .OrderBy(marker => marker.Start)
            .ToList();

        if (styleMarkers.Count == 0)
        {
            return defaultStyle is not null && defaultStyle != "NORMAL"
                ? [new ZwiftSauceRoadStyle { Start = -1, End = 2, Style = defaultStyle }]
                : [];
        }

        var normalizedStyles = new List<ZwiftSauceRoadStyle>();
        for (var index = 0; index < styleMarkers.Count; index++)
        {
            var (Start, End, StyleId, Style) = styleMarkers[index];
            var end = index + 1 < styleMarkers.Count
                ? Math.Min(End, styleMarkers[index + 1].Start)
                : End;

            if (end > Start)
            {
                normalizedStyles.Add(new ZwiftSauceRoadStyle
                {
                    Start = Start,
                    End = end,
                    Style = Style,
                });
            }
        }

        if (defaultStyle is not null && defaultStyle != "NORMAL" && styleMarkers.Any(marker => marker.Style != defaultStyle))
        {
            var cursor = -1d;
            foreach (var marker in normalizedStyles.OrderBy(marker => marker.Start))
            {
                if (marker.Start > cursor)
                {
                    normalizedStyles.Add(new ZwiftSauceRoadStyle { Start = cursor, End = marker.Start, Style = defaultStyle });
                }

                cursor = Math.Max(cursor, marker.End);
            }

            if (cursor < 2)
            {
                normalizedStyles.Add(new ZwiftSauceRoadStyle { Start = cursor, End = 2, Style = defaultStyle });
            }
        }

        var mergedStyles = new List<ZwiftSauceRoadStyle>();
        foreach (var style in normalizedStyles.OrderBy(style => style.Start))
        {
            var previous = mergedStyles.LastOrDefault();
            if (previous is not null && previous.Style == style.Style && style.Start <= previous.End)
            {
                mergedStyles[^1] = previous with { End = Math.Max(previous.End, style.End) };
            }
            else
            {
                mergedStyles.Add(style);
            }
        }

        return [.. mergedStyles];
    }

    private static ZwiftSauceRoadPathPoint CreatePathPoint(ZwiftXmlObjectRoadWorldRoadEntityElement entity)
    {
        var position = ParseVector(entity.Position!);

        return new ZwiftSauceRoadPathPoint
        {
            X = position[0],
            Z = position[2],
            Y = position[1],
            Straight = entity.Straight ? true : null,
            TanIn = ParseOptionalVector(entity.TangentIn),
            TanOut = ParseOptionalVector(entity.TangentOut)
        };
    }

    private static double[]? ParseOptionalVector(string? value)
    {
        if (value is null)
        {
            return null;
        }

        var vector = ParseVector(value);
        return [vector[0], vector[2], vector[1]];
    }

    private static double[] ParseVector(string value)
    {
        return [.. value.Trim('{', '}').Split(',').Select(part => double.Parse(part, CultureInfo.InvariantCulture))];
    }

    private static string? GetDefaultStyle(int? style, Dictionary<int, string> roadStyles)
    {
        if (style is not { } styleId)
        {
            return null;
        }

        return roadStyles.TryGetValue(styleId, out var styleName) ? styleName : null;
    }

    private static string[] GetSports(int? allowedSport)
    {
        var sports = new List<string>();
        if ((allowedSport.GetValueOrDefault() & 1) != 0)
        {
            sports.Add("cycling");
        }

        if ((allowedSport.GetValueOrDefault() & 2) != 0)
        {
            sports.Add("running");
        }

        if ((allowedSport.GetValueOrDefault() & 4) != 0)
        {
            sports.Add("rowing");
        }

        return [.. sports];
    }
}
