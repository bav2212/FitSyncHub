using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FitSyncHub.Zwift.Xml.Abstractions;

namespace FitSyncHub.Zwift.Xml;

public class ZwiftXmlObjectRootParser<T> : IDisposable
    where T : IZwiftXmlObjectRoot
{
    private record XmlSerializerWithPropertyInfoTuple
    {
        public required XmlSerializer XmlSerializer { get; init; }
        public required PropertyInfo PropertyInfo { get; init; }
    }

    private readonly Dictionary<string, XmlSerializerWithPropertyInfoTuple> _xmlPropertyMapping = [];
    private readonly List<UnknownXmlElementInfo> _unknownXmlElements = [];
    private bool _disposedValue;

    public ZwiftXmlObjectRootParser()
    {
        foreach (var propertyInfo in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var xmlRootAttribute = propertyInfo.PropertyType.GetCustomAttribute<XmlRootAttribute>()
                ?? throw new Exception("set xml root attibute");
            var xmlPropertyName = xmlRootAttribute.ElementName;

            var serializer = new XmlSerializer(propertyInfo.PropertyType);
            SetHandlersToLogUnknownEvents(serializer, xmlPropertyName);

            _xmlPropertyMapping[xmlPropertyName] = new XmlSerializerWithPropertyInfoTuple
            {
                XmlSerializer = serializer,
                PropertyInfo = propertyInfo
            };
        }
    }

    public T Parse(string filePath)
    {
        var result = Activator.CreateInstance<T>();

        var xml = File.ReadAllText(filePath);
        var declaration = xml.IndexOf("<?xml", StringComparison.Ordinal);
        if (declaration > 0)
        {
            xml = xml[declaration..];
        }

        using var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings
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

            if (_xmlPropertyMapping.TryGetValue(xmlElementName, out var tuple))
            {
                using var sub = reader.ReadSubtree();
                sub.Read(); // move into element

                var deserializedObject = tuple.XmlSerializer.Deserialize(sub)
                    ?? throw new InvalidDataException($"Can't deserialize {xmlElementName}");

                tuple.PropertyInfo?.SetValue(result, deserializedObject);
            }
            else
            {
                _unknownXmlElements.Add(new UnknownXmlElementInfo
                {
                    ElementName = xmlElementName,
                    ElementValue = reader.Value,
                    PropertyName = "",
                    Reason = UnknownXmlElementReason.UnknownElement
                });
            }
        }

        ValidateRequiredMembers(result);

        return result;
    }

    public void ValidateRequiredMembers(T obj)
    {
        foreach (var (_, tuple) in _xmlPropertyMapping)
        {
            var propertyInfo = tuple.PropertyInfo;
            if (!propertyInfo.IsDefined(typeof(System.Runtime.CompilerServices.RequiredMemberAttribute), inherit: true))
            {
                continue;
            }

            var value = propertyInfo.GetValue(obj);

            var missing = value == null ||
                (propertyInfo.PropertyType.IsValueType &&
                value.Equals(Activator.CreateInstance(propertyInfo.PropertyType)));

            if (missing)
            {
                throw new InvalidOperationException(
                    $"Required property '{propertyInfo.Name}' is not set.");
            }
        }
    }

    private void SetHandlersToLogUnknownEvents(XmlSerializer serializer, string xmlPropertyName)
    {
        serializer.UnknownAttribute += (sender, args) =>
            _unknownXmlElements.Add(new UnknownXmlElementInfo
            {
                Reason = UnknownXmlElementReason.UnknownAttribute,
                PropertyName = xmlPropertyName,
                ElementName = args.Attr.Name,
                ElementValue = args.Attr.Value
            });

        serializer.UnknownNode += (sender, args) =>
        {
            if (args.NodeType == XmlNodeType.Attribute)
            {
                return; // will be handled by UnknownAttribute
            }

            if (args.NodeType == XmlNodeType.Element)
            {
                return; // will be handled by UnknownELement
            }

            _unknownXmlElements.Add(new UnknownXmlElementInfo
            {
                Reason = UnknownXmlElementReason.UnknownNode,
                PropertyName = xmlPropertyName,
                ElementName = args.Name,
                ElementValue = args.Text ?? string.Empty
            });
        };

        serializer.UnknownElement += (sender, args) =>
            _unknownXmlElements.Add(new UnknownXmlElementInfo
            {
                Reason = UnknownXmlElementReason.UnknownElement,
                PropertyName = xmlPropertyName,
                ElementName = args.Element.Name,
                ElementValue = args.Element.InnerXml
            });

        serializer.UnreferencedObject += (sender, args) =>
            _unknownXmlElements.Add(new UnknownXmlElementInfo
            {
                Reason = UnknownXmlElementReason.UnreferencedObject,
                PropertyName = xmlPropertyName,
                ElementName = args.UnreferencedId ?? throw new InvalidOperationException("Unreferenced ID is null"),
                ElementValue = args.UnreferencedObject?.ToString() ?? throw new InvalidOperationException("Unreferenced object is null")
            });
    }

    private void EnsureNoUnknownElements()
    {
        if (_unknownXmlElements.Count == 0)
        {
            return;
        }

        var formmattedText = _unknownXmlElements
            .GroupBy(x => new { x.Reason, x.PropertyName, x.ElementName })
            .Select(x => new
            {
                x.Key.Reason,
                x.Key.PropertyName,
                x.Key.ElementName,
                Values = x.Select(e => e.ElementValue).Distinct().ToArray()
            })
            .Aggregate(new StringBuilder(), (acc, curr) =>
            {
                acc.AppendLine($"Reason: {curr.Reason}, Property: {curr.PropertyName}, Element: {curr.ElementName}, Values: {string.Join(", ", curr.Values)}");
                return acc;
            }, sb => sb.ToString());

        throw new InvalidDataException($"Unknown XML elements found during deserialization:{Environment.NewLine}{formmattedText}");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
#if DEBUG
                EnsureNoUnknownElements();
#endif
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private enum UnknownXmlElementReason
    {
        UnknownAttribute,
        UnknownNode,
        UnknownElement,
        UnreferencedObject
    }

    private readonly record struct UnknownXmlElementInfo
    {
        public required UnknownXmlElementReason Reason { get; init; }
        public required string PropertyName { get; init; }
        public required string ElementName { get; init; }
        public required string ElementValue { get; init; }
    }
}
