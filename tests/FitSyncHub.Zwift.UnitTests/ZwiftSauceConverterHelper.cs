using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace FitSyncHub.Zwift.UnitTests;

internal class ZwiftSauceConverterHelper
{
    internal static JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                static typeInfo =>
                {
                    if (typeInfo.Kind != JsonTypeInfoKind.Object)
                    {
                        return;
                    }

                    foreach (var propertyInfo in typeInfo.Properties)
                    {
                        // Strip IsRequired constraint from every property.
                        propertyInfo.IsRequired = false;
                    }
                }
            }
        }
    };
}
