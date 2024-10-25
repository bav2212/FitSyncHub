using System.Collections;
using System.Net;
using System.Reflection;

namespace StravaWebhooksAzureFunctions.Extensions;

public static class CookieCollectionExtensions
{
    public static CookieCollection GetAllCookies(this CookieContainer container)
    {
        var allCookies = new CookieCollection();
        var domainTable = (Hashtable)container.GetType()
            .GetRuntimeFields()
            .First(x => x.Name == "m_domainTable")
            .GetValue(container)!;

        var pathListField = default(FieldInfo);
        foreach (var domain in domainTable.Values)
        {
            var pathList = (SortedList)(pathListField ??= domain.GetType()
                .GetRuntimeFields()
                .First(x => x.Name == "m_list"))
                .GetValue(domain)!;

            foreach (CookieCollection cookies in pathList.GetValueList())
            {
                allCookies.Add(cookies);
            }
        }
        return allCookies;
    }
}
