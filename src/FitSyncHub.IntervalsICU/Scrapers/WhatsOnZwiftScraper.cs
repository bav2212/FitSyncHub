using HtmlAgilityPack;

namespace FitSyncHub.IntervalsICU.Scrapers;

public static class WhatsOnZwiftScraper
{
    public static async Task<WhatsOnZwiftScrapedResponse> ScrapeWorkoutStructure(Uri uri)
    {
        var web = new HtmlWeb();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var htmlPageDoc = await web.LoadFromWebAsync(uri, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var workoutList = ParseWorkoutList(htmlPageDoc).ToList();

        return new WhatsOnZwiftScrapedResponse()
        {
            NameSegments = [.. ParseNameSegments(htmlPageDoc)],
            WorkoutList = workoutList,
        };
    }

    public static async Task<List<string>> ScrapeWorkoutPlanLinks(Uri uri)
    {
        var web = new HtmlWeb();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var htmlPageDoc = await web.LoadFromWebAsync(uri, null, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        var viewWorkoutButtons = htmlPageDoc.DocumentNode
            .SelectNodes("//a[@class=\"button\"]")
            .Where(x => x.InnerText == "View workout")
            .ToList();

        return viewWorkoutButtons
            .ConvertAll(x => x.Attributes["href"].Value);
    }

    private static IEnumerable<string> ParseNameSegments(HtmlDocument htmlPageDoc)
    {
        var breadcrumbs = htmlPageDoc.DocumentNode
            .SelectSingleNode("//div[@class=\"breadcrumbs\"]");

        var breadcrumbSegments = breadcrumbs.InnerText
            .Replace("\n", "")
            .Split("&raquo;", StringSplitOptions.TrimEntries)
            .Select(HtmlEntity.DeEntitize);

        return breadcrumbSegments.Skip(1);
    }

    private static IEnumerable<string> ParseWorkoutList(HtmlDocument htmlPageDoc)
    {
        var workoutListNode = htmlPageDoc.DocumentNode
            .SelectSingleNode("//div[@class=\"one-third column workoutlist\"]");

        // Load the HTML content
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(workoutListNode.OuterHtml);

        // Select all divs with class 'textbar'
        var textbarDivs = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'textbar')]");

        if (textbarDivs is null)
        {
            yield break;
        }

        // Iterate over each textbar div and extract the relevant information
        foreach (var div in textbarDivs)
        {
            // Get the inner text of the div (this will contain the time and other details)
            var divText = div.InnerText.Trim();

            // hack to avoid too much changes in code
            yield return divText.Replace("rpm,", "rpm");
        }
    }
}
