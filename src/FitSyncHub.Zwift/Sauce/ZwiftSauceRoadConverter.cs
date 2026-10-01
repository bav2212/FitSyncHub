using System.Globalization;
using FitSyncHub.Zwift.Xml.Models;
using FitSyncHub.Zwift.Xml.Models.Road;

namespace FitSyncHub.Zwift.Sauce;

public class ZwiftSauceRoadConverter
{
    private static readonly IReadOnlyDictionary<int, string> DefaultStyleOverrides = new Dictionary<int, string>
    {
        [17] = "GRASS"
    };

    public static ZwiftSauceRoad[] Convert(
        ZwiftXmlObjectRoadRoot zwiftRoadRoot,
        ZwiftXmlObjectRoadStyleRoot zwiftRoadStyleRoot)
    {
        var zwiftRoadStyles = zwiftRoadStyleRoot.Roads.Segments
            .Select(x => x.Style!)
            .ToList();

        return [.. zwiftRoadRoot.World.Roads
            .Where(road => road.AllowedSport.GetValueOrDefault() != 0)
            .Select(road => new ZwiftSauceRoad
        {
            DefaultStyle = GetDefaultStyle(road.DefaultStyle, zwiftRoadStyles),
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
            Styles = []
        })];
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

    private static string? GetDefaultStyle(int? style, IReadOnlyList<string> roadStyles)
    {
        if (style is not { } styleId)
        {
            return null;
        }

        if (DefaultStyleOverrides.TryGetValue(styleId, out var overrideStyle))
        {
            return overrideStyle;
        }

        return styleId >= 0 && styleId < roadStyles.Count ? roadStyles[styleId] : null;
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
