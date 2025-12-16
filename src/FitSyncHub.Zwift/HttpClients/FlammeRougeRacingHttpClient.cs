using System.Text.Json;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Zwift.HttpClients;

public enum FlammeRougeRacingCategory
{
    CAP,
    DRA,
    CRP,
    GHT,
    HAB,
    BON,
    CAY,
    JLP,
    PEP,
    BEL,
}

public class FlammeRougeRacingHttpClient
{
    private readonly HttpClient _httpClient;

    public FlammeRougeRacingHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<long>> GetTourRegisteredRiders(FlammeRougeRacingCategory flammeRougeRacingCategory, CancellationToken cancellationToken)
    {
        const string BaseUrl = "https://flammerougeracing.com/wp-admin/admin-ajax.php";
        const int TableId = 105;

        var url = QueryHelpers.AddQueryString(BaseUrl, new Dictionary<string, StringValues>
        {
            {"action", "get_wdtable"},
            {"table_id", TableId.ToString() },
        });

        var wdtNonce = await GetWdtNonce(TableId, cancellationToken);

        var start = 0;
        const int Page = 25;
        List<long> riderIds = [];

        while (true)
        {
            var formData = new Dictionary<string, string>()
            {
                {"draw", "3" },
                {"columns[4][data]", "4" },
                {"columns[4][name]", "RACINGFRHC" },
                {"columns[4][searchable]", "true" },
                {"columns[4][orderable]", "false" },
                {"columns[4][search][value]", flammeRougeRacingCategory.ToString() },
                {"columns[4][search][regex]", "true" },
                {"start", $"{start}" },
                {"length", $"{Page}" },
                {"wdtNonce", wdtNonce },
            };

            var dataContent = new FormUrlEncodedContent(formData);
            var response = await _httpClient.PostAsync(url, dataContent, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var jsonDocument = JsonDocument.Parse(content);
            var ridersData = jsonDocument.RootElement.GetProperty("data");

            foreach (var riderData in ridersData.EnumerateArray())
            {
                var zwiftPowerAnchorElement = riderData.EnumerateArray().ToArray()[2].GetString()!;

                var doc = new HtmlDocument();
                doc.LoadHtml(zwiftPowerAnchorElement);

                // select the <a> element
                var linkNode = doc.DocumentNode.SelectSingleNode("//a");

                var href = linkNode.GetAttributeValue("href", "");

                // parse the "z" parameter
                var uri = new Uri(href);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);

                var profileId = query["z"];
                if (string.IsNullOrWhiteSpace(profileId) || !long.TryParse(profileId, out var parserProfileId))
                {
                    throw new InvalidOperationException("can't parse profileId");
                }

                riderIds.Add(long.Parse(profileId));
            }

            if (ridersData.GetArrayLength() % Page != 0)
            {
                break;
            }

            start += Page;
        }

        return riderIds;
    }

    private async Task<string> GetWdtNonce(int tableId, CancellationToken cancellationToken)
    {
        const string Url = "https://flammerougeracing.com/tour-registered/";
        var response = await _httpClient.GetAsync(Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var doc = new HtmlDocument();
        doc.LoadHtml(content);

        // Build full id pattern: wdtNonceFrontendServerSide_SUFFIX
        var id = $"wdtNonceFrontendServerSide_{tableId}";

        var node = doc.DocumentNode
            .SelectSingleNode($"//input[@id='{id}']");

        var nonce = node?.GetAttributeValue("value", null!);
        if (string.IsNullOrWhiteSpace(nonce))
        {
            throw new InvalidOperationException("wdtNonce is null or empty");
        }

        return nonce;
    }
}
