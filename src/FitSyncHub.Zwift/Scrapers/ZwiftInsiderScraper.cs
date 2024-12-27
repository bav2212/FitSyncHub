using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace FitSyncHub.Zwift.Scrapers;
public class ZwiftInsiderScraper
{
    public static async Task<ZwiftInsiderScraperResponse> ScrapeZwiftInsiderWorkoutPage(Uri uri)
    {
        var web = new HtmlWeb();
        var htmlPageDoc = await web.LoadFromWebAsync(uri, null, null);

        var wattPerKgResult = ParseWattPerKg(htmlPageDoc);
        var leadInAndElevationResult = ParseLeadInAndElevation(htmlPageDoc);

        return new ZwiftInsiderScraperResponse
        {
            WattPerKg = wattPerKgResult,
            LeadInAndElevation = leadInAndElevationResult
        };
    }

    private static ZwiftInsiderScraperWattPerKgElapsedTimeItemsResponse? ParseWattPerKg(
        HtmlDocument htmlPageDoc)
    {
        // Find the node containing "Time Estimates"
        var timeEstimatesNode = htmlPageDoc.DocumentNode.SelectSingleNode("//*[contains(text(), 'Time Estimates')]");

        if (timeEstimatesNode != null)
        {
            // Find the parent node with class 'wp-block-toolset-blocks-container'
            var containerNode = timeEstimatesNode
                .Ancestors("div") // Look at all parent <div> elements
                .FirstOrDefault(div => div.GetClasses().Contains("wp-block-toolset-blocks-container"));

            if (containerNode == null)
            {
                return default;
            }

            var items = GetWattPerKgValues(containerNode.InnerHtml).ToList();

            return new ZwiftInsiderScraperWattPerKgElapsedTimeItemsResponse
            {
                WattsPerKdTimeEstimate = items.ToDictionary(x => x.WattPerKg, x => x.Minutes)
            };
        }

        return default;
    }

    private static IEnumerable<ZwiftInsiderScraperWattPerKgElapsedTimeResponse> GetWattPerKgValues(
        string html)
    {
        // Load the HTML document
        var document = new HtmlDocument();
        document.LoadHtml(html);

        // Find the <p> tag with time estimates
        var timeEstimatesNode = document.DocumentNode.SelectSingleNode("//p[contains(., 'Time Estimates')]");
        if (timeEstimatesNode == null)
        {
            yield break;
        }

        // Extract the inner text
        var timeEstimatesText = timeEstimatesNode.InnerText;

        // Regular expression to match the time estimates
        var pattern = @"(\d+)\sW/kg:\s(\d+)\sminutes";
        var matches = Regex.Matches(timeEstimatesText, pattern);

        // Parse and print the results
        foreach (Match match in matches)
        {
            var power = match.Groups[1].Value;
            var minutes = match.Groups[2].Value;

            yield return new ZwiftInsiderScraperWattPerKgElapsedTimeResponse
            {
                WattPerKg = int.Parse(power),
                Minutes = double.Parse(minutes)
            };
        }
    }

    private static ZwiftInsiderScraperLeadInAndElevationResponse? ParseLeadInAndElevation(
        HtmlDocument htmlPageDoc)
    {
        // XPath to find <p> elements with all specified classes
        var xpath = $"//p[contains(@class, 'has-text-align-center') and " +
                           $"contains(@class, 'has-white-color') and " +
                           $"contains(@class, 'has-text-color') and " +
                           $"contains(@class, 'has-link-color')]";

        var paragraphNode = htmlPageDoc.DocumentNode.SelectSingleNode(xpath);
        return GetLeadInAndElevation(paragraphNode.OuterHtml);
    }

    private static ZwiftInsiderScraperLeadInAndElevationResponse? GetLeadInAndElevation(string html)
    {
        // Load HTML into HtmlDocument
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        // XPath to find the span element containing the text
        var xpath = "//span[contains(text(), 'lead-in')]";

        // Select the node
        var node = htmlDoc.DocumentNode.SelectSingleNode(xpath);

        if (node == null)
        {
            return default;
        }

        // Extract the text content
        var segment = node.InnerText.Trim();

        // Parse the length and elevation using regular expressions
        var pattern = @"\+([\d\.]+)km.*?lead-in(?:.*?([\d\.]+)m.*elevation)?$";
        var match = Regex.Match(segment, pattern);

        if (!match.Success)
        {
            return default;
        }

        var length = double.Parse(match.Groups[1].Value);
        var elevation = match.Groups[2].Success
            ? double.Parse(match.Groups[2].Value)
            : 0;

        return new ZwiftInsiderScraperLeadInAndElevationResponse
        {
            Length = length,
            Elevation = elevation,
        };
    }
}
