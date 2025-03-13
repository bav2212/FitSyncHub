using System.Text.Json;
using FitSyncHub.Functions.JsonSerializerContexts;

namespace FitSyncHub.GarminConnect.UnitTests;

public class GarminConnectSerializationUnitTest
{
    [Theory]
    [ClassData(typeof(GarminActivitiesTestData))]
    public async Task GarminActivityResponse_Deserialization_WorkCorrectly(string garminActivityFilePath)
    {
        var content = await File.ReadAllTextAsync(garminActivityFilePath);

        JsonSerializer.Deserialize(content, GarminConnectCamelCaseSerializerContext.Default.GarminActivityResponse);
    }

    private class GarminActivitiesTestData : TheoryData<string>
    {
        public GarminActivitiesTestData()
        {
            var files = Directory.EnumerateFiles("Data/GarminActivities").ToArray();
            AddRange(files);
        }
    }
}
