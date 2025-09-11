namespace FitSyncHub.Xert;

public sealed class XertWorkoutFormat
{
    private readonly string _value;

    private XertWorkoutFormat(string value)
    {
        _value = value;
    }

    public static XertWorkoutFormat ERG => new("erg");
    public static XertWorkoutFormat ZWO => new("zwo");
    public static XertWorkoutFormat MRC => new("mrc");

    public override string ToString() => _value;
}
