namespace FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class JsonProjectionDependsOnAttribute(params string[] propertyNames) : Attribute
{
    public IReadOnlyList<string> PropertyNames { get; } = propertyNames;
}
