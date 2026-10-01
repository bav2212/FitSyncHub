using System.Text.Json;
using System.Text.Json.Serialization;

namespace FitSyncHub.Zwift.Sauce;

public sealed record ZwiftSauceRoad
{
	public string? DefaultStyle { get; init; }
	public int Id { get; init; }
	public bool IsAvailable { get; init; }
	public bool Looped { get; init; }
	public bool OneWay { get; init; }
	public ZwiftSauceRoadPathPoint[] Path { get; init; } = [];
	public JsonElement[] Segments { get; init; } = [];
	public string? SplineType { get; init; }
	public string[] Sports { get; init; } = [];
	public ZwiftSauceRoadStyle[] Styles { get; init; } = [];
}


public sealed record ZwiftSauceRoadStyle
{
	public double Start { get; init; }
	public double End { get; init; }
	public string? Style { get; init; }
}

[JsonConverter(typeof(ZwiftSauceRoadPathPointJsonConverter))]
public sealed record ZwiftSauceRoadPathPoint
{
	public double X { get; init; }
	public double Z { get; init; }
	public double Y { get; init; }
	public bool? Straight { get; init; }
	public double[]? TanIn { get; init; }
	public double[]? TanOut { get; init; }
}

public sealed class ZwiftSauceRoadPathPointJsonConverter : JsonConverter<ZwiftSauceRoadPathPoint>
{
	public override ZwiftSauceRoadPathPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
		{
			throw new JsonException("A road path point must be an array.");
		}

		reader.Read();
		var x = reader.GetDouble();
		reader.Read();
		var z = reader.GetDouble();
		reader.Read();
		var y = reader.GetDouble();
		reader.Read();

		bool? straight = null;
		double[]? tanIn = null;
		double[]? tanOut = null;

		if (reader.TokenType == JsonTokenType.StartObject)
		{
			while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
			{
				var propertyName = reader.GetString();
				reader.Read();
				switch (propertyName)
				{
					case "straight":
						straight = reader.GetBoolean();
						break;
					case "tanIn":
						tanIn = ReadVector(ref reader);
						break;
					case "tanOut":
						tanOut = ReadVector(ref reader);
						break;
					default:
						reader.Skip();
						break;
				}
			}

			reader.Read();
		}

		if (reader.TokenType != JsonTokenType.EndArray)
		{
			throw new JsonException("A road path point must end with an array token.");
		}

		return new ZwiftSauceRoadPathPoint
		{
			X = x,
			Z = z,
			Y = y,
			Straight = straight,
			TanIn = tanIn,
			TanOut = tanOut
		};
	}

	public override void Write(Utf8JsonWriter writer, ZwiftSauceRoadPathPoint value, JsonSerializerOptions options)
	{
		writer.WriteStartArray();
		writer.WriteNumberValue(value.X);
		writer.WriteNumberValue(value.Z);
		writer.WriteNumberValue(value.Y);

		if (value.Straight.HasValue || value.TanIn is not null || value.TanOut is not null)
		{
			writer.WriteStartObject();
			if (value.Straight is { } straight)
			{
				writer.WriteBoolean("straight", straight);
			}

			if (value.TanIn is { } tanIn)
			{
				writer.WritePropertyName("tanIn");
				JsonSerializer.Serialize(writer, tanIn, options);
			}

			if (value.TanOut is { } tanOut)
			{
				writer.WritePropertyName("tanOut");
				JsonSerializer.Serialize(writer, tanOut, options);
			}

			writer.WriteEndObject();
		}

		writer.WriteEndArray();
	}

	private static double[] ReadVector(ref Utf8JsonReader reader)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
		{
			throw new JsonException("A tangent must be an array.");
		}

		var values = new List<double>();
		while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
		{
			values.Add(reader.GetDouble());
		}

		return [.. values];
	}
}
