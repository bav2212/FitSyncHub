using System.Text.Json;
using FitSyncHub.GarminConnect.JsonSerializerContexts;

namespace FitSyncHub.GarminConnect.UnitTests;

public class GarminConnectSerializationUnitTest
{
    [Theory]
    [ClassData(typeof(GarminActivitiesTestData))]
    public async Task GarminActivitySearchResponse_Deserialization_WorkCorrectly(string garminActivityFilePath)
    {
        var content = await File.ReadAllTextAsync(garminActivityFilePath);

        JsonSerializer.Deserialize(content, GarminConnectActivityListSerializerContext.Default.GarminActivitySearchResponse);
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
