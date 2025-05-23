using System.Text.Json;
using FitSyncHub.GarminConnect.JsonSerializerContexts;

namespace FitSyncHub.GarminConnect.UnitTests;

public class GarminConnectSerializationUnitTest
{
    [Theory]
    [ClassData(typeof(GarminActivityListTestData))]
    public async Task GarminActivitySearchResponse_Deserialization_WorkCorrectly(string garminActivityFilePath)
    {
        var content = await File.ReadAllTextAsync(garminActivityFilePath);

        JsonSerializer.Deserialize(content, GarminConnectActivityListSerializerContext.Default.GarminActivitySearchResponse);
    }

    private class GarminActivityListTestData : TheoryData<string>
    {
        public GarminActivityListTestData()
        {
            var files = Directory.EnumerateFiles("Data/GarminActivityList").ToArray();
            AddRange(files);
        }
    }

    [Theory]
    [ClassData(typeof(GarminActivityTestData))]
    public async Task GarminActivityResponse_Deserialization_WorkCorrectly(string garminActivityFilePath)
    {
        var content = await File.ReadAllTextAsync(garminActivityFilePath);

        JsonSerializer.Deserialize(content, GarminConnectActivitySerializerContext.Default.GarminActivityResponse);
    }

    private class GarminActivityTestData : TheoryData<string>
    {
        public GarminActivityTestData()
        {
            var files = Directory.EnumerateFiles("Data/GarminActivity").ToArray();
            AddRange(files);
        }
    }
}
