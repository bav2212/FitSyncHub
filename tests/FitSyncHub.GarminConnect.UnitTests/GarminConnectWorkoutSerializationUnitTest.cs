using System.Text.Json;
using FitSyncHub.GarminConnect.JsonSerializerContexts;

namespace FitSyncHub.GarminConnect.UnitTests;

public class GarminConnectWorkoutSerializationUnitTest
{
    [Theory]
    [ClassData(typeof(GarminWorkoutTestData))]
    public async Task GarminConnectWorkoutResponse_Deserialization_WorkCorrectly(string filePath)
    {
        var content = await File.ReadAllTextAsync(filePath);

        var value = JsonSerializer.Deserialize(content,
            GarminConnectWorkoutSerializerContext.Default.GarminConnectWorkoutResponse);

        Assert.NotNull(value);
    }

    private class GarminWorkoutTestData : TheoryData<string>
    {
        public GarminWorkoutTestData()
        {
            var files = Directory.EnumerateFiles("Data/Workouts").ToArray();
            AddRange(files);
        }
    }
}
