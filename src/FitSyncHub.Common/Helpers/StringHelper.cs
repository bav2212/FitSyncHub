using FitSyncHub.Common.Models;

namespace FitSyncHub.Common.Helpers;

public static class StringHelper
{
    public static string? Sanitize(string? str)
    {
        return str?.Replace("\r", string.Empty).Replace("\n", string.Empty);
    }
}
