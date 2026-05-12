using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using FitSyncHub.Zwift.Xml.Abstractions;

namespace FitSyncHub.Zwift.Xml;

public class ZwiftXmlObjectRootParser<T> where T : IZwiftXmlObjectRoot
{
    private readonly Dictionary<string, XmlSerializer> _serializers = [];

    public ZwiftXmlObjectRootParser()
    {
        foreach (var propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propertyName = propertyInfo.Name;
            var xmlPropertyName = char.ToLower(propertyName[0]) + propertyName[1..];

            var serializer = new XmlSerializer(propertyInfo.PropertyType);
            SetHandlersToLogUnknownEvents(serializer, xmlPropertyName);

            _serializers[xmlPropertyName] = serializer;
        }
    }

    public T Parse(string filePath)
    {
        var result = Activator.CreateInstance<T>();

        using var reader = XmlReader.Create(filePath, new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreWhitespace = true,
            // THIS is the important part: allow multiple top-level elements
            ConformanceLevel = ConformanceLevel.Fragment,
        });

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                continue;
            }

            var xmlElementName = reader.Name;

            if (_serializers.TryGetValue(xmlElementName, out var serializer))
            {
                using var sub = reader.ReadSubtree();
                sub.Read(); // move into element

                var deserializedObject = serializer.Deserialize(sub)
                    ?? throw new InvalidDataException($"Can't deserialize {xmlElementName}");

                var propertyInfo = typeof(T).GetProperty(
                        char.ToUpper(xmlElementName[0]) + xmlElementName[1..],
                        BindingFlags.Public | BindingFlags.Instance);
                propertyInfo?.SetValue(result, deserializedObject);
            }
        }

        ValidateRequiredMembers(result);

        return result;
    }

    public static void ValidateRequiredMembers(T obj)
    {
        var type = obj.GetType();

        var requiredProperties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.IsDefined(typeof(System.Runtime.CompilerServices.RequiredMemberAttribute), inherit: true));

        foreach (var prop in requiredProperties)
        {
            var value = prop.GetValue(obj);

            var missing = value == null ||
                (prop.PropertyType.IsValueType &&
                value.Equals(Activator.CreateInstance(prop.PropertyType)));

            if (missing)
            {
                throw new InvalidOperationException(
                    $"Required property '{prop.Name}' is not set.");
            }
        }
    }

    private static void SetHandlersToLogUnknownEvents(XmlSerializer serializer, string xmlPropertyName)
    {
        serializer.UnknownAttribute += (sender, args) =>
            Console.WriteLine("Property {0}: Unknown attribute {1}=\'{2}\'", xmlPropertyName, args.Attr.Name, args.Attr.Value);

        serializer.UnknownNode += (sender, args) =>
            Console.WriteLine("Property {0}: Unknown Node:{1}\t{2}", xmlPropertyName, args.Name, args.Text);

        serializer.UnknownElement += (sender, args) =>
            Console.WriteLine("Property {0}: Unknown Element:{1}\t{2}", xmlPropertyName, args.Element.Name, args.Element.InnerXml);

        serializer.UnreferencedObject += (sender, args) =>
            Console.WriteLine("Property {0}: Unreferenced Object: {1}\t{2}", xmlPropertyName, args.UnreferencedId, args.UnreferencedObject.ToString());
    }
}
