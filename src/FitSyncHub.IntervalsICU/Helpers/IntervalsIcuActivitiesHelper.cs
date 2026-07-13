using FitSyncHub.IntervalsICU.HttpClients.Models.Responses;

namespace FitSyncHub.IntervalsICU.Helpers;

public class IntervalsIcuActivitiesHelper
{
    public static List<List<ActivityResponse>> GroupConsecutiveActivities(List<ActivityResponse> activities)
    {
        return [.. activities
           .OrderBy(a => a.StartDateLocal)
           .GroupBy(a => a.StartDateLocal.Date)
           .SelectMany(dayGroup =>
           {
               var result = new List<List<ActivityResponse>>();
               List<ActivityResponse>? currentGroup = null;

               foreach (var activity in dayGroup.OrderBy(a => a.StartDateLocal))
               {
                   if (currentGroup == null)
                   {
                       currentGroup = [activity];
                       result.Add(currentGroup);
                       continue;
                   }

                   var lastActivity = currentGroup[^1];
                   var gap = activity.EndTimeLocal - lastActivity.StartDateLocal;

                   if (gap.TotalHours > 2)
                   {
                       currentGroup = [];
                       result.Add(currentGroup);
                   }

                   currentGroup.Add(activity);
               }

               return result;
           })];
    }
}
