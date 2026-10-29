using System.Globalization;
using FitSyncHub.Zwift.Xml.Models;
using FitSyncHub.Zwift.Xml.Models.Road;

namespace FitSyncHub.Zwift.Sauce;

public class ZwiftSauceRoadConverter
{
    public static ZwiftSauceRoad[] Convert(
        uint worldId,
        ZwiftXmlObjectRoadRoot zwiftRoadRoot,
        ZwiftXmlObjectRoadStyleRoot zwiftRoadStyleRoot)
    {
        _ = worldId;

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
        var styles = road.Entities
            .Where(entity => entity.Type == "ENTITY_TYPE_ROADMARKER"
                && entity.StyleSpecified
                && styleMapping.ContainsKey(entity.Style)
                && entity.Style != defaultStyleId)
            .Select(entity => new ZwiftSauceRoadStyle
            {
                Start = entity.RoadTime1Specified ? entity.RoadTime1 : -1,
                End = entity.RoadTime2Specified ? entity.RoadTime2 : 2,
                Style = styleMapping[entity.Style]
            })
            .ToArray();

        if (defaultStyleId.HasValue
            && styleMapping.TryGetValue(defaultStyleId.Value, out var defaultStyle)
            && defaultStyle != "NORMAL")
        {
            if (styles.Length == 0)
            {
                return [new ZwiftSauceRoadStyle { Start = -1, End = 2, Style = defaultStyle }];
            }

            var mergedStyles = new List<ZwiftSauceRoadStyle>();
            foreach (var style in styles.OrderBy(item => item.Start))
            {
                var previous = mergedStyles.LastOrDefault();
                if (previous is not null
                    && previous.Style == style.Style
                    && style.Start <= previous.End)
                {
                    mergedStyles[^1] = previous with { End = Math.Max(previous.End, style.End) };
                }
                else
                {
                    mergedStyles.Add(style);
                }
            }

            var cursor = mergedStyles.Max(item => item.End);
            if (cursor < 2)
            {
                mergedStyles.Add(new ZwiftSauceRoadStyle { Start = cursor, End = 2, Style = defaultStyle });
            }

            return [.. mergedStyles.OrderBy(item => item.Start)];
        }

        return styles;
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
