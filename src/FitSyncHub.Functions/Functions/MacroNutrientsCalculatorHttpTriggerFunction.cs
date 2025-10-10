using FitSyncHub.Common.Extensions;
using FitSyncHub.GarminConnect.HttpClients;
using FitSyncHub.GarminConnect.Models.Responses;
using FitSyncHub.IntervalsICU.HttpClients;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using DateTime = System.DateTime;

namespace FitSyncHub.Functions.Functions;

public class MacroNutrientsCalculatorHttpTriggerFunction
{
    private readonly IntervalsIcuHttpClient _intervalsIcuHttpClient;
    private readonly GarminConnectHttpClient _garminConnectHttpClient;
    private readonly ILogger<MacroNutrientsCalculatorHttpTriggerFunction> _logger;

    public MacroNutrientsCalculatorHttpTriggerFunction(
        IntervalsIcuHttpClient intervalsIcuHttpClient,
        GarminConnectHttpClient garminConnectHttpClient,
        ILogger<MacroNutrientsCalculatorHttpTriggerFunction> logger)
    {
        _intervalsIcuHttpClient = intervalsIcuHttpClient;
        _garminConnectHttpClient = garminConnectHttpClient;
        _logger = logger;
    }

    [Function(nameof(MacroNutrientsCalculatorHttpTriggerFunction))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "macro-nutrients-calculator")] HttpRequest req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        string? optimalEnergyAvailabilityQueryParameter = req.Query["oea"];
        if (optimalEnergyAvailabilityQueryParameter is null)
        {
            _logger.LogInformation("optimal energy availability is not set. Set 'OEA' parameter");
            return new BadRequestObjectResult("optimal energy availability is not set. Set 'OEA' parameter");
        }

        if (!int.TryParse(optimalEnergyAvailabilityQueryParameter, out var optimalEvengyAvailability))
        {
            _logger.LogInformation("OEA has wrong format");
            return new BadRequestObjectResult("OEA has wrong format");
        }

        if (optimalEvengyAvailability == 0)
        {
            _logger.LogInformation("OEA should be more than 0");
            return new BadRequestObjectResult("OEA should be more than 0");
        }

        string? dateQueryParameter = req.Query["date"];
        if (!DateOnly.TryParse(dateQueryParameter, out var date))
        {
            date = DateOnly.FromDateTime(DateTime.Today);
        }

        var activeKiloCalories = await GetCompletedActivitiesKiloCalories(date, cancellationToken);
        var plannedKiloCalories = await GetPlannedActivitiesKiloCalories(date, cancellationToken);
        var totalKiloCalories = activeKiloCalories + plannedKiloCalories;

        var (weightInKg, bodyFat) = await GetWeightAndBodyFat(date, cancellationToken);

        var macroNutrients = MacroNutrientsCalculator
            .Calculate(optimalEvengyAvailability, weightInKg, bodyFat, totalKiloCalories);

        var resultString = $"Carbs: {macroNutrients.Carbs:0}, protein: {macroNutrients.Protein:0}, fat: {macroNutrients.Fat:0}, kCal: {macroNutrients.Calories:0}";

        return new OkObjectResult(resultString);
    }

    private async Task<float> GetCompletedActivitiesKiloCalories(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var activities = await _intervalsIcuHttpClient.ListActivities(new(date, date), cancellationToken);

        // human efficiency is almost similar to 0.25, so we can use joules instead of kcal
        return activities
            .Select(x => x?.IcuJoules)
            .WhereNotNull()
            .Sum() / 1000.0f;
    }

    private async Task<float> GetPlannedActivitiesKiloCalories(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var events = await _intervalsIcuHttpClient.ListEvents(new(date, date), cancellationToken);
        var plannedEvents = events.Where(x => x.PairedActivityId is null).ToList();

        float plannedCalories = 0;
        foreach (var plannedEvent in plannedEvents)
        {
            if (plannedEvent.Joules is { } joules)
            {
                // human efficiency is almost similar to 0.25, so we can use joules instead of kcal
                plannedCalories += joules;
                continue;
            }

            if (plannedEvent.WorkoutDocument?.WorkCalculated is not null)
            {
                plannedCalories += plannedEvent.WorkoutDocument.WorkCalculated;
                continue;
            }
        }

        return plannedCalories / 1000.0f;
    }

    private async Task<(float weightKg, float bodyFat)> GetWeightAndBodyFat(
        DateOnly date,
        CancellationToken cancellationToken)
    {
        // that's possible that I doesn't measure weight for few days, so we iterating over last 10 days until get data
        GarminWeightResponse garminWeightResponse;
        var attempt = 0;
        do
        {
            var nextDate = date.AddDays(-attempt++);
            garminWeightResponse = await _garminConnectHttpClient.GetWeightDayView(nextDate, cancellationToken);
        }
        while (attempt < 10 && garminWeightResponse.DateWeightList.Length == 0);

        if (garminWeightResponse.DateWeightList.Length == 0)
        {
            throw new Exception("No weight data for last 10 days");
        }

        var garminWeight = garminWeightResponse.DateWeightList[^1];

        var weightKg = garminWeight.Weight / 1000;
        var bodyFat = (garminWeight.BodyFat ?? throw new Exception("No body fat")) / 100;

        return (weightKg, bodyFat);
    }
}

internal static class MacroNutrientsCalculator
{
    // 65% carbs 20% fats 14% protein 1% spices/additives
    // https://www.cyclingapps.net/blog/in-search-of-the-optimal-diet-for-longevity/
    private const float CarbsRatio = 0.65f;
    private const float ProteinRatio = 0.15f;
    private const float FatRatio = 0.20f;

    // trainerroad .com recommends 65% carbs, 17% fats, 18% protein
    //private const float CarbsRatio = 0.65f;
    //private const float ProteinRatio = 0.18f;
    //private const float FatRatio = 0.17f;

    private const int CarbsEnergyCapacity = 4;
    private const int ProteinEnergyCapacity = 4;
    private const int FatEnergyCapacity = 9;

    internal static MacroNutrientsResponse Calculate(
        int optimalEnergyAvailability,
        float weightKg,
        float bodyFat,
        float workoutCalories)
    {
        var bmrCalories = GetBasalMetabolicRateMacroNutrients(optimalEnergyAvailability, weightKg, bodyFat);

        return CalculateMacroNutrientsForBasalMetabolicRate(bmrCalories)
            + CalculateMacroNutrientsForRecoveryAfterWorkouts(workoutCalories);
    }

    private static float GetBasalMetabolicRateMacroNutrients(
        int optimalEnergyAvailability,
        float weightKg,
        float bodyFat)
    {
        var fatFreeMass = weightKg * (1 - bodyFat);
        return optimalEnergyAvailability * fatFreeMass;
    }

    private static MacroNutrientsResponse CalculateMacroNutrientsForBasalMetabolicRate(float baseCalories)
    {
        return new MacroNutrientsResponse
        {
            Calories = baseCalories,
            Carbs = baseCalories * CarbsRatio / CarbsEnergyCapacity,
            Protein = baseCalories * ProteinRatio / ProteinEnergyCapacity,
            Fat = baseCalories * FatRatio / FatEnergyCapacity,
        };
    }

    private static MacroNutrientsResponse CalculateMacroNutrientsForRecoveryAfterWorkouts(float workoutCalories)
    {
        return new MacroNutrientsResponse
        {
            Calories = workoutCalories,
            Carbs = workoutCalories * 0.8f / CarbsEnergyCapacity,
            Protein = workoutCalories * 0.2f / ProteinEnergyCapacity,
            Fat = 0,
        };
    }
}

public record MacroNutrientsResponse
{
    public required float Calories { get; init; }
    public required float Carbs { get; init; }
    public required float Protein { get; init; }
    public required float Fat { get; init; }

    public static MacroNutrientsResponse operator +(MacroNutrientsResponse first, MacroNutrientsResponse second)
        => new()
        {
            Calories = first.Calories + second.Calories,
            Carbs = first.Carbs + second.Carbs,
            Fat = first.Fat + second.Fat,
            Protein = first.Protein + second.Protein
        };
}
