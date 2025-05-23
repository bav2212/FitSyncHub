using System.Text.Json;
using FitSyncHub.Common.Fit;
using Microsoft.Extensions.Logging;
using Moq;

namespace FitSyncHub.Common.UnitTests;

public class TssCalculatorUnitTest
{
    [Theory]
    [ClassData(typeof(TssTestData))]
    public void Calculate_WorkCorrectly(string fitFilePath, string jsonFilePath)
    {
        var mock = new Mock<ILogger<FitFileDecoder>>();
        var decoder = new FitFileDecoder(mock.Object).Decode(fitFilePath);

        var json = File.ReadAllText(jsonFilePath);

        var activityInfo = JsonSerializer.Deserialize<ActivityInfo>(json)!;
        var calculationResult = TssCalculator.Calculate(decoder, activityInfo.Ftp);
        Assert.NotNull(calculationResult);

        Assert.Equal(activityInfo.MovingTime, calculationResult.Duration);
        Assert.Equal(activityInfo.Tss, calculationResult.Tss, 1.5);
    }

    private class TssTestData : TheoryData<string, string>
    {
        public TssTestData()
        {
            foreach (var activityRootDirectoryPath in Directory.EnumerateDirectories("FitData"))
            {
                var files = Directory.EnumerateFiles(activityRootDirectoryPath);

                var fitFile = files.Single(x => x.EndsWith(".fit"));
                var jsonFile = files.Single(x => x.EndsWith(".json"));

                Add(fitFile, jsonFile);
            }
        }
    }
}
