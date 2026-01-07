using System.Globalization;
using System.Text.Json;
using FitSyncHub.Zwift.Models.FRR;
using HtmlAgilityPack;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace FitSyncHub.Zwift.HttpClients;

public class FlammeRougeRacingHttpClient
{
    private const string TableDataUrl = "https://flammerougeracing.com/wp-admin/admin-ajax.php";

    private const int TableIdTourRegistered = 105;
    private const int TableIdTourResultsGC = 142;

    private readonly Dictionary<int, string> _wdtNonceMapping = new()
    {
        { TableIdTourRegistered, "https://flammerougeracing.com/tour-registered"  },
        { TableIdTourResultsGC, "https://flammerougeracing.com/tour-results-gc" },
    };

    private readonly HttpClient _httpClient;
    public FlammeRougeRacingHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<long>> GetTourRegisteredRiders(FlammeRougeRacingCategory flammeRougeRacingCategory, CancellationToken cancellationToken)
    {
        const int TableId = TableIdTourRegistered;

        var url = QueryHelpers.AddQueryString(TableDataUrl, new Dictionary<string, StringValues>
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

    public async Task<List<FlammeRougeRacingEGapResultModel>> GetStageEGap(
        FlammeRougeRacingCategory flammeRougeRacingCategory,
        int stageNumber,
        CancellationToken cancellationToken)
    {
        const int TableId = TableIdTourResultsGC;

        var url = QueryHelpers.AddQueryString(TableDataUrl, new Dictionary<string, StringValues>
        {
            {"action", "get_wdtable"},
            {"table_id", TableId.ToString() },
        });

        var wdtNonce = await GetWdtNonce(TableId, cancellationToken);

        var start = 0;
        const int Page = 25;
        List<FlammeRougeRacingEGapResultModel> result = [];

        while (true)
        {
            var formData = new Dictionary<string, string>()
            {
                {"draw", "4"},
                {"columns[0][data]", "0"},
                {"columns[0][name]", "SERIES"},
                {"columns[0][searchable]", "true"},
                {"columns[0][orderable]", "false"},
                {"columns[0][search][value]", ""},
                {"columns[0][search][regex]", "false"},
                {"columns[1][data]", "1"},
                {"columns[1][name]", "EVENTREF"},
                {"columns[1][searchable]", "true"},
                {"columns[1][orderable]", "false"},
                {"columns[1][search][value]", ""},
                {"columns[1][search][regex]", "false"},
                {"columns[2][data]", "2"},
                {"columns[2][name]", "CLASS"},
                {"columns[2][searchable]", "true"},
                {"columns[2][orderable]", "false"},
                {"columns[2][search][value]", $"M-{flammeRougeRacingCategory}"},
                {"columns[2][search][regex]", "true"},
                {"columns[3][data]", "3"},
                {"columns[3][name]", "STAGE"},
                {"columns[3][searchable]", "true"},
                {"columns[3][orderable]", "false"},
                {"columns[3][search][value]", stageNumber.ToString()},
                {"columns[3][search][regex]", "true"},
                {"columns[4][data]", "4"},
                {"columns[4][name]", "GENDER"},
                {"columns[4][searchable]", "true"},
                {"columns[4][orderable]", "false"},
                {"columns[4][search][value]", ""},
                {"columns[4][search][regex]", "false"},
                {"columns[5][data]", "5"},
                {"columns[5][name]", "IRLGEN"},
                {"columns[5][searchable]", "true"},
                {"columns[5][orderable]", "false"},
                {"columns[5][search][value]", ""},
                {"columns[5][search][regex]", "false"},
                {"columns[6][data]", "6"},
                {"columns[6][name]", "RCLASS"},
                {"columns[6][searchable]", "true"},
                {"columns[6][orderable]", "false"},
                {"columns[6][search][value]", ""},
                {"columns[6][search][regex]", "false"},
                {"columns[7][data]", "7"},
                {"columns[7][name]", "POSITION"},
                {"columns[7][searchable]", "true"},
                {"columns[7][orderable]", "false"},
                {"columns[7][search][value]", ""},
                {"columns[7][search][regex]", "false"},
                {"columns[8][data]", "8"},
                {"columns[8][name]", "RIDER"},
                {"columns[8][searchable]", "true"},
                {"columns[8][orderable]", "false"},
                {"columns[8][search][value]", ""},
                {"columns[8][search][regex]", "false"},
                {"columns[9][data]", "9"},
                {"columns[9][name]", "CLUB"},
                {"columns[9][searchable]", "true"},
                {"columns[9][orderable]", "false"},
                {"columns[9][search][value]", ""},
                {"columns[9][search][regex]", "false"},
                {"columns[10][data]", "10"},
                {"columns[10][name]", "RAGE"},
                {"columns[10][searchable]", "true"},
                {"columns[10][orderable]", "false"},
                {"columns[10][search][value]", ""},
                {"columns[10][search][regex]", "false"},
                {"columns[11][data]", "11"},
                {"columns[11][name]", "RIDERID"},
                {"columns[11][searchable]", "true"},
                {"columns[11][orderable]", "false"},
                {"columns[11][search][value]", ""},
                {"columns[11][search][regex]", "false"},
                {"columns[12][data]", "12"},
                {"columns[12][name]", "ZPwr"},
                {"columns[12][searchable]", "true"},
                {"columns[12][orderable]", "false"},
                {"columns[12][search][value]", ""},
                {"columns[12][search][regex]", "false"},
                {"columns[13][data]", "13"},
                {"columns[13][name]", "ZRapp"},
                {"columns[13][searchable]", "true"},
                {"columns[13][orderable]", "false"},
                {"columns[13][search][value]", ""},
                {"columns[13][search][regex]", "false"},
                {"columns[14][data]", "14"},
                {"columns[14][name]", "RTIME"},
                {"columns[14][searchable]", "true"},
                {"columns[14][orderable]", "false"},
                {"columns[14][search][value]", ""},
                {"columns[14][search][regex]", "false"},
                {"columns[15][data]", "15"},
                {"columns[15][name]", "STAGETOTAL"},
                {"columns[15][searchable]", "true"},
                {"columns[15][orderable]", "false"},
                {"columns[15][search][value]", ""},
                {"columns[15][search][regex]", "false"},
                {"columns[16][data]", "16"},
                {"columns[16][name]", "RTTIME"},
                {"columns[16][searchable]", "true"},
                {"columns[16][orderable]", "false"},
                {"columns[16][search][value]", ""},
                {"columns[16][search][regex]", "false"},
                {"columns[17][data]", "17"},
                {"columns[17][name]", "TOTALTIME"},
                {"columns[17][searchable]", "true"},
                {"columns[17][orderable]", "false"},
                {"columns[17][search][value]", ""},
                {"columns[17][search][regex]", "false"},
                {"columns[18][data]", "18"},
                {"columns[18][name]", "TEGAP"},
                {"columns[18][searchable]", "true"},
                {"columns[18][orderable]", "false"},
                {"columns[18][search][value]", ""},
                {"columns[18][search][regex]", "false"},
                {"columns[19][data]", "19"},
                {"columns[19][name]", "CUMULATIVE_TIMEDIF"},
                {"columns[19][searchable]", "true"},
                {"columns[19][orderable]", "false"},
                {"columns[19][search][value]", ""},
                {"columns[19][search][regex]", "false"},
                {"columns[20][data]", "20"},
                {"columns[20][name]", "UPD"},
                {"columns[20][searchable]", "true"},
                {"columns[20][orderable]", "false"},
                {"columns[20][search][value]", ""},
                {"columns[20][search][regex]", "false"},
                {"columns[21][data]", "21"},
                {"columns[21][name]", "FPEGAP"},
                {"columns[21][searchable]", "true"},
                {"columns[21][orderable]", "false"},
                {"columns[21][search][value]", ""},
                {"columns[21][search][regex]", "false"},
                {"columns[22][data]", "22"},
                {"columns[22][name]", "EGAP"},
                {"columns[22][searchable]", "true"},
                {"columns[22][orderable]", "false"},
                {"columns[22][search][value]", ""},
                {"columns[22][search][regex]", "false"},
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
                var riderDataArray = riderData.EnumerateArray().ToArray();

                var frrEGapResult = new FlammeRougeRacingEGapResultModel
                {
                    Position = int.Parse(riderDataArray[7].GetString()!, NumberStyles.AllowThousands, CultureInfo.InvariantCulture),
                    Rider = riderDataArray[8].GetString()!,
                    RiderId = long.Parse(riderDataArray[11].GetString()!, NumberStyles.AllowThousands, CultureInfo.InvariantCulture),
                    EGap = riderDataArray[22].GetString()!,
                };
                result.Add(frrEGapResult);
            }

            if (ridersData.GetArrayLength() % Page != 0)
            {
                break;
            }

            start += Page;
        }

        return result;
    }

    private async Task<string> GetWdtNonce(int tableId, CancellationToken cancellationToken)
    {
        var url = _wdtNonceMapping[tableId];
        var response = await _httpClient.GetAsync(url, cancellationToken);
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
