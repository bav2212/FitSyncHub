using System.Globalization;
using System.Text;
using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace FitSyncHub.Functions.Helpers;

public static class IntervalsIcuResponseOverviewHelper
{
    public static string ToStringOverview(params IEnumerable<EventResponse> events)
    {
        return events
            .Where(x => x.PairedActivityId == null)
            .Aggregate(new StringBuilder(),
                (sb, x) =>
                {
                    var abbreviatedDayName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(x.StartDateLocal.DayOfWeek);
                    sb.Append(abbreviatedDayName);
                    sb.Append('\t');

                    sb.Append($"{x.Type}: {x.Name}");
                    if (x.MovingTime.HasValue)
                    {
                        var timeSpan = TimeSpan.FromSeconds(x.MovingTime.Value);
                        sb.Append(", ");

                        if (timeSpan.Hours > 0)
                        {
                            sb.Append(timeSpan.Hours);
                            sb.Append('h');
                        }

                        if (timeSpan.Minutes > 0)
                        {
                            sb.Append(timeSpan.Minutes);
                            sb.Append('m');
                        }
                    }

                    if (x.IcuTrainingLoad.HasValue)
                    {
                        sb.Append($", Load {x.IcuTrainingLoad.Value}");
                    }

                    return sb.AppendLine();
                },
                sb => sb.ToString());
    }
}
