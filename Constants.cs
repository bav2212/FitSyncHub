using StravaWebhooksAzureFunctions.Models;

namespace StravaWebhooksAzureFunctions;

public class Constants
{
    public const long MyAthleteId = 50156776;
    // Marin Nicasio 2 id
    public const string MyCityBikeGearId = "b9320665";

    public static Boundaries MyMapBoundaries => new(
        26.098528,
        50.1026653,
        26.129767,
        50.1384141
    );

    public class StravaActivityType
    {
        public const string AlpineSki = "AlpineSki";
        public const string BackcountrySki = "BackcountrySki";
        public const string Canoeing = "Canoeing";
        public const string Crossfit = "Crossfit";
        public const string EBikeRide = "EBikeRide";
        public const string Elliptical = "Elliptical";
        public const string Golf = "Golf";
        public const string Handcycle = "Handcycle";
        public const string Hike = "Hike";
        public const string IceSkate = "IceSkate";
        public const string InlineSkate = "InlineSkate";
        public const string Kayaking = "Kayaking";
        public const string Kitesurf = "Kitesurf";
        public const string NordicSki = "NordicSki";
        public const string Ride = "Ride";
        public const string RockClimbing = "RockClimbing";
        public const string RollerSki = "RollerSki";
        public const string Rowing = "Rowing";
        public const string Run = "Run";
        public const string Sail = "Sail";
        public const string Skateboard = "Skateboard";
        public const string Snowboard = "Snowboard";
        public const string Snowshoe = "Snowshoe";
        public const string Soccer = "Soccer";
        public const string StairStepper = "StairStepper";
        public const string StandUpPaddling = "StandUpPaddling";
        public const string Surfing = "Surfing";
        public const string Swim = "Swim";
        public const string Velomobile = "Velomobile";
        public const string VirtualRide = "VirtualRide";
        public const string VirtualRun = "VirtualRun";
        public const string Walk = "Walk";
        public const string WeightTraining = "WeightTraining";
        public const string Wheelchair = "Wheelchair";
        public const string Windsurf = "Windsurf";
        public const string Workout = "Workout";
        public const string Yoga = "Yoga";
    }
}
