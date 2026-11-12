using System.Text.Json;
using FitSyncHub.Common.Fit;
using Microsoft.Extensions.Logging;
using Moq;

namespace FitSyncHub.Common.UnitTests;

public sealed class TssCalculatorUnitTest
{
    [Theory]
    [ClassData(typeof(TssTestData))]
    public void Calculate_WorkCorrectly(string fitFilePath, string jsonFilePath)
    {
        var mock = new Mock<ILogger<FitFileDecoder>>();
        var decoder = new FitFileDecoder(mock.Object).Decode(fitFilePath);

        var json = File.ReadAllText(jsonFilePath);

        var activityInfo = JsonSerializer.Deserialize<ActivityInfo>(json)!;
        var calculationResult = TssCalculator.Calculate(decoder, activityInfo.Ftp) switch
        {
            CalculateTssResult result => result,
            NoMovingPowerData => throw new Xunit.Sdk.XunitException("Expected moving power data"),
            InvalidFtp invalidFtp => throw new Xunit.Sdk.XunitException($"Unexpected FTP: {invalidFtp.Ftp}")
        };

        Assert.Equal(activityInfo.MovingTime, calculationResult.Duration);
        Assert.Equal(activityInfo.Tss, calculationResult.Tss, 1.5);
    }

    [Theory]
    [ClassData(typeof(TssTestData))]
    public void Calculate_WithInvalidFtp_ReturnsInvalidFtp(string fitFilePath, string _)
    {
        var mock = new Mock<ILogger<FitFileDecoder>>();
        var decoder = new FitFileDecoder(mock.Object).Decode(fitFilePath);

        var calculationResult = TssCalculator.Calculate(decoder, 0);

        Assert.IsType<InvalidFtp>(calculationResult.Value);
    }

    private sealed class TssTestData : TheoryData<string, string>
    {
        public TssTestData()
        {
            foreach (var activityRootDirectoryPath in Directory.EnumerateDirectories("Data\\FitData"))
            {
                var files = Directory.EnumerateFiles(activityRootDirectoryPath);

                var fitFile = files.Single(x => x.EndsWith(".fit"));
                var jsonFile = files.Single(x => x.EndsWith(".json"));

                Add(fitFile, jsonFile);
            }
        }
    }
}
