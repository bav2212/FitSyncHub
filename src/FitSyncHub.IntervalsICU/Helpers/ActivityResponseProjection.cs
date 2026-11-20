using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FitSyncHub.IntervalsICU.HttpClients.Models;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace FitSyncHub.IntervalsICU.Helpers;

internal static class ActivityResponseProjection
{
    public static (string Fields, Func<JsonElement, T?> Deserialize) Create<T>(
        Expression<Func<ActivityResponse, T>> selector)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(selector);

        var context = IntervalsIcuSnakeCaseSourceGenerationContext.Default;
        var fields = GetFields(selector, context.ActivityResponse);
        fields.Add("source");

        var options = new JsonSerializerOptions(context.Options)
        {
            TypeInfoResolver = context.WithAddedModifier(typeInfo =>
            {
                if (typeInfo.Type != typeof(ActivityResponse))
                {
                    return;
                }

                foreach (var property in typeInfo.Properties)
                {
                    // Intervals.icu omits null values in a projected response.
                    property.IsRequired = property.IsRequired && fields.Contains(property.Name) &&
                        !property.IsSetNullable;
                }
            })
        };

        var contract = options.GetTypeInfo<ActivityResponse>();
        var project = selector.Compile();
        return (string.Join(",", fields), row => project(row.Deserialize(contract)!));
    }

    private static HashSet<string> GetFields<T>(
        Expression<Func<ActivityResponse, T>> selector, JsonTypeInfo<ActivityResponse> contract)
    {
        if (selector.Body is not NewExpression projection || projection.Arguments.Count == 0)
        {
            throw new ArgumentException("Select properties with an expression like activity => new { activity.Id }.",
                nameof(selector));
        }

        var fields = new HashSet<string>(StringComparer.Ordinal);
        foreach (var argument in projection.Arguments)
        {
            if (argument is not MemberExpression member ||
                member.Expression != selector.Parameters[0] ||
                member.Member is not PropertyInfo property)
            {
                throw new ArgumentException("Select direct ActivityResponse properties.", nameof(selector));
            }

            var propertyNames = property.GetCustomAttribute<JsonProjectionDependsOnAttribute>()?.PropertyNames
                ?? [property.Name];
            foreach (var propertyName in propertyNames)
            {
                var jsonProperty = contract.Properties.SingleOrDefault(item =>
                    item.Get is not null &&
                    item.AttributeProvider is PropertyInfo clrProperty &&
                    clrProperty.Name == propertyName)
                    ?? throw new ArgumentException($"{propertyName} is not an activity JSON field.", nameof(selector));
                fields.Add(jsonProperty.Name);
            }
        }

        return fields;
    }
}
