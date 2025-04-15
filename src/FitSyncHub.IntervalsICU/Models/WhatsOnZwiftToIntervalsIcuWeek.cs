namespace FitSyncHub.IntervalsICU.Models;

public record WhatsOnZwiftToIntervalsIcuWeek
{
    public required bool IsPreparationWeek { get; init; }
    public required int WeekNumber { get; init; }

    public static WhatsOnZwiftToIntervalsIcuWeek Create(int weekNumber)
    {
        return new WhatsOnZwiftToIntervalsIcuWeek { WeekNumber = weekNumber, IsPreparationWeek = false };
    }

    public static WhatsOnZwiftToIntervalsIcuWeek CreateWeek1()
    {
        return Create(1);
    }

    public static WhatsOnZwiftToIntervalsIcuWeek CreatePreparationWeek()
    {
        return new WhatsOnZwiftToIntervalsIcuWeek { WeekNumber = default, IsPreparationWeek = true };
    }
}
