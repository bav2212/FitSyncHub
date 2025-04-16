using FitSyncHub.IntervalsICU.Models;
using HtmlAgilityPack;

namespace FitSyncHub.IntervalsICU.Services;

public class WhatsOnZwiftScraperService
{
    private readonly HttpClient _client;

    public WhatsOnZwiftScraperService(HttpClient client)
    {
        _client = client;
    }

    public async Task<WhatsOnZwiftScrapedResult> ScrapeWorkoutStructure(Uri uri, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var htmlPageDoc = new HtmlDocument();
        htmlPageDoc.LoadHtml(content);

        var workoutList = ParseWorkoutList(htmlPageDoc).ToList();

        return new WhatsOnZwiftScrapedResult()
        {
            NameSegments = [.. ParseNameSegments(htmlPageDoc)],
            WorkoutList = workoutList,
        };
    }

    public async Task<List<string>> ScrapeWorkoutPlanLinks(Uri uri, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var htmlPageDoc = new HtmlDocument();
        htmlPageDoc.LoadHtml(content);

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
        var textBarDivBlocks = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'textbar')]");

        if (textBarDivBlocks is null)
        {
            yield break;
        }

        // Iterate over each textbar div and extract the relevant information
        foreach (var div in textBarDivBlocks)
        {
            // Get the inner text of the div (this will contain the time and other details)
            var divText = div.InnerText.Trim();

            // hack to avoid too much changes in code
            yield return divText.Replace("rpm,", "rpm");
        }
    }
}
