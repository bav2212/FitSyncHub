using System.Net;
using FitSyncHub.IntervalsICU.Models;
using FitSyncHub.IntervalsICU.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace FitSyncHub.IntervalsIcu.UnitTests;

public sealed class WhatsOnZwiftToIntervalsIcuServiceUnitTest
{
    [Fact]
    public async Task ScrapeAndConvertToIntervalsIcu_WhenWorkoutIsValid_ReturnsConversionResult()
    {
        using var httpClient = new HttpClient(new StaticResponseHandler(
            "<html><nav aria-label=\"Breadcrumbs\">Home&raquo;Week 1&raquo;Day 1 - Workout</nav>" +
            "<div class=\"textbar\">5min @ 50% FTP</div></html>"));
        var scraper = new WhatsOnZwiftScraperService(httpClient);
        var service = new WhatsOnZwiftToIntervalsIcuService(
            scraper,
            NullLogger<WhatsOnZwiftToIntervalsIcuService>.Instance);

        var result = await service.ScrapeAndConvertToIntervalsIcu(
            new Uri("https://example.com/workout"),
            CancellationToken.None);

        var workout = Assert.IsType<WhatsOnZwiftToIntervalsIcuConvertResult>(result.Value);
        Assert.NotEmpty(workout.IntervalsIcuWorkoutDescription);
    }

    [Fact]
    public async Task ScrapeAndConvertToIntervalsIcu_WhenBreadcrumbsAreMissing_ReturnsConversionError()
    {
        using var httpClient = new HttpClient(new StaticResponseHandler("<html></html>"));
        var scraper = new WhatsOnZwiftScraperService(httpClient);
        var service = new WhatsOnZwiftToIntervalsIcuService(
            scraper,
            NullLogger<WhatsOnZwiftToIntervalsIcuService>.Instance);

        var result = await service.ScrapeAndConvertToIntervalsIcu(
            new Uri("https://example.com/workout"),
            CancellationToken.None);

        Assert.IsType<WhatsOnZwiftConversionError>(result.Value);
    }

    private sealed class StaticResponseHandler(string content) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }
}
