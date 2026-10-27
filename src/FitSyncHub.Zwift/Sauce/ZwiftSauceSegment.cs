namespace FitSyncHub.Zwift.Sauce;

public sealed record ZwiftSauceSegment
{
	public int? ArchId { get; init; }
	public required string Color { get; init; }
	public required double Distance { get; init; }
	public required bool GivesPowerUp { get; init; }
	public required string Id { get; init; }
	public required string Name { get; init; }
	public required bool Reverse { get; init; }
	public required double RoadFinish { get; init; }
	public required int RoadId { get; init; }
	public required double RoadStart { get; init; }
	public required uint WorldId { get; init; }
}
