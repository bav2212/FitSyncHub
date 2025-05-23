using System.Text.Json;
using FitSyncHub.Common.Workouts;
using FitSyncHub.GarminConnect.Converters;
using FitSyncHub.GarminConnect.JsonSerializerContexts;
using FitSyncHub.GarminConnect.Services;
using Moq;

namespace FitSyncHub.GarminConnect.UnitTests;

public class GarminConnectToIntervalsIcuWorkoutConvertionServiceUnitTest
{
    private const int Ftp = 286;

    [Fact]
    public async Task GarminConnectToIntervalsIcuWorkoutConvertionService_Convertion_WorkCorrectly()
    {
        var content = await File.ReadAllTextAsync("Data/Workouts/workout_9458067d-b80c-4eb1-aab4-691d5c9e3713.json");
        var workoutResponse = JsonSerializer.Deserialize(content, GarminConnectWorkoutSerializerContext.Default.GarminWorkoutResponse)!;

        // Arrange
        var mockInitializer = new Mock<IGarminConnectToIntervalsIcuWorkoutStepConverterInitializer>();
        mockInitializer.Setup(x => x.Initialize(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new GarminConnectToIntervalsIcuRideWorkoutStepConverter(Ftp));

        var accessor = new Mock<Func<WorkoutType, IGarminConnectToIntervalsIcuWorkoutStepConverterInitializer>>();
        accessor.Setup(f => f(It.IsAny<WorkoutType>())).Returns(mockInitializer.Object);

        var service = new GarminConnectToInternalWorkoutConverterService(accessor.Object);

        var workout = await service.Convert(workoutResponse, default);
        var workoutSteps = workout.Steps;

        // has three groups
        Assert.Equal(3, workoutSteps.Count);

        // not finished
        Assert.Collection(workoutSteps,
            AssertWarmupInterval,
            _ => { },
            __ => { }
        );
    }

    private static void AssertWarmupInterval(WorkoutStep step)
    {
        Assert.Equal(WorkoutStepType.Warmup, step.Type);
        var rideStep = Assert.IsType<RideWorkoutStep>(step);

        Assert.Equal(TimeSpan.FromMinutes(20), rideStep.Time);

        var ftpRange = Assert.IsType<RideFtpRange>(rideStep.Ftp);
        Assert.Equal(55, ftpRange.From);
        Assert.Equal(75, ftpRange.To);
    }
}
