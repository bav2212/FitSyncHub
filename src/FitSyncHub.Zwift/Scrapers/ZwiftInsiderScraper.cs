using HtmlAgilityPack;

namespace FitSyncHub.Zwift.Scrapers;
public class ZwiftInsiderScraper
{
    public static async Task<ZwiftInsiderScraperResponse> ScrapeZwiftInsiderWorkoutPage(Uri uri)
    {
        var web = new HtmlWeb();
        var htmlPageDoc = await web.LoadFromWebAsync(uri, null, null);

        // XPath to find <p> elements with all specified classes
        var xpath = $"//p[contains(@class, 'has-text-align-center') and " +
                       $"contains(@class, 'has-white-color') and " +
                       $"contains(@class, 'has-text-color') and " +
                       $"contains(@class, 'has-link-color')]";

        var paragraphNode = htmlPageDoc.DocumentNode.SelectSingleNode(xpath);
        var (length, elevation) = ParseLeadInAndElevation(paragraphNode.OuterHtml);

        return new ZwiftInsiderScraperResponse
        {
            Length = length,
            Elevation = elevation
        };
    }

    private static (double length, double elevation) ParseLeadInAndElevation(string paragraphNodeHtml)
    {
        // Load HTML into HtmlDocument
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(paragraphNodeHtml);

        // XPath to find the span element containing the text
        var xpath = "//span[contains(text(), 'lead-in')]";

        // Select the node
        var node = htmlDoc.DocumentNode.SelectSingleNode(xpath);

        if (node == null)
        {
            return (0, 0);
        }

        // Extract the text content
        var segment = node.InnerText.Trim();

        // Parse the length and elevation using regular expressions
        var pattern = @"\+([\d\.]+)km.*?lead-in(?:.*?([\d\.]+)m.*elevation)?$";
        var match = System.Text.RegularExpressions.Regex.Match(segment, pattern);

        if (match.Success)
        {
            var length = double.Parse(match.Groups[1].Value);
            var elevation = match.Groups[2].Success
                ? double.Parse(match.Groups[2].Value)
                : 0;
            return (length, elevation);
        }
        else
        {
            return (0, 0);
        }
    }
}
