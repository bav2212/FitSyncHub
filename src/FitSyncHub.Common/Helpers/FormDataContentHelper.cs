using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using FitSyncHub.Common.Abstractions;
using FitSyncHub.Common.Models;

namespace FitSyncHub.Common.Helpers;

public static class FormDataContentHelper
{
    public static MultipartFormDataContent CreateMultipartFormDataContent<TValue>(
        FileModel fileModel,
        TValue value,
        JsonTypeInfo<TValue> jsonTypeInfo) where TValue : IFormDataValue
    {
        var serializedObject = JsonSerializer.Serialize(value, jsonTypeInfo)!;
        var jsonDocument = JsonDocument.Parse(serializedObject);

        var formData = new MultipartFormDataContent
        {
            // Add the file to the form-data with the key 'file'
            { new ByteArrayContent(fileModel.Bytes), "file", fileModel.Name }
        };

        foreach (var jsonProperty in jsonDocument.RootElement.EnumerateObject())
        {
            var propertyName = jsonProperty.Name;
            var propertyValue = jsonProperty.Value;

            if (propertyValue.ValueKind is not JsonValueKind.String
                and not JsonValueKind.True
                and not JsonValueKind.False
                and not JsonValueKind.Number)
            {
                continue;
            }

            var stringValue = propertyValue.ValueKind switch
            {
                JsonValueKind.String => propertyValue.GetString(),
                _ => propertyValue.GetRawText(),
            };

            if (stringValue is null)
            {
                continue;
            }

            formData.Add(new StringContent(stringValue), propertyName);
        }

        return formData;
    }
}
