using System.Text.RegularExpressions;
using FitSyncHub.ZwiftInsider.Models;
using HtmlAgilityPack;

namespace FitSyncHub.ZwiftInsider.Services;

public partial class ZwiftInsiderScraperService
{
    private readonly HttpClient _httpClient;

    public ZwiftInsiderScraperService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ZwiftInsiderScraperResult> ScrapeZwiftInsiderWorkoutPage(Uri uri, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var htmlPageDoc = new HtmlDocument();
        htmlPageDoc.LoadHtml(content);

        var wattPerKgResult = ParseWattPerKg(htmlPageDoc);
        var leadInAndElevationResult = ParseLeadInAndElevation(htmlPageDoc);

        return new ZwiftInsiderScraperResult
        {
            WattPerKg = wattPerKgResult,
            LeadInAndElevation = leadInAndElevationResult
        };
    }

    private static ZwiftInsiderScraperWattPerKgElapsedTimeItemsResult? ParseWattPerKg(
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

            return new ZwiftInsiderScraperWattPerKgElapsedTimeItemsResult
            {
                WattsPerKdTimeEstimate = items.ToDictionary(x => x.WattPerKg, x => x.Minutes)
            };
        }

        return default;
    }

    private static IEnumerable<ZwiftInsiderScraperWattPerKgElapsedTimeResult> GetWattPerKgValues(
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

        // Parse and print the results
        foreach (Match match in WattsPerKgAndMinutesPattern().Matches(timeEstimatesText))
        {
            var power = match.Groups[1].Value;
            var minutes = match.Groups[2].Value;

            yield return new ZwiftInsiderScraperWattPerKgElapsedTimeResult
            {
                WattPerKg = int.Parse(power),
                Minutes = double.Parse(minutes)
            };
        }
    }

    private static ZwiftInsiderScraperLeadInAndElevationResult? ParseLeadInAndElevation(
        HtmlDocument htmlPageDoc)
    {
        // XPath to find <p> elements with all specified classes
        const string Xpath = "//p[contains(@class, 'has-text-align-center') and " +
                           "contains(@class, 'has-white-color') and " +
                           "contains(@class, 'has-text-color') and " +
                           "contains(@class, 'has-link-color')]";

        var paragraphNode = htmlPageDoc.DocumentNode.SelectSingleNode(Xpath);
        return GetLeadInAndElevation(paragraphNode.OuterHtml);
    }

    private static ZwiftInsiderScraperLeadInAndElevationResult? GetLeadInAndElevation(string html)
    {
        // Load HTML into HtmlDocument
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        // XPath to find the span element containing the text
        const string Xpath = "//span[contains(text(), 'lead-in')]";

        // Select the node
        var node = htmlDoc.DocumentNode.SelectSingleNode(Xpath);

        if (node == null)
        {
            return default;
        }

        // Extract the text content
        var segment = node.InnerText.Trim();

        // Parse the length and elevation using regular expressions
        var match = LeadInDisatancePattern().Match(segment);

        if (!match.Success)
        {
            return default;
        }

        var length = double.Parse(match.Groups[1].Value);
        var elevation = match.Groups[2].Success
            ? double.Parse(match.Groups[2].Value)
            : 0;

        return new ZwiftInsiderScraperLeadInAndElevationResult
        {
            Length = length,
            Elevation = elevation,
        };
    }

    [GeneratedRegex(@"(\d+)\sW/kg:\s(\d+)\sminutes")]
    private static partial Regex WattsPerKgAndMinutesPattern();
    [GeneratedRegex(@"\+([\d\.]+)km.*?lead-in(?:.*?([\d\.]+)m.*elevation)?$")]
    private static partial Regex LeadInDisatancePattern();
}
