using System.Net;
using System.Text.Json;
using FitSyncHub.IntervalsICU.HttpClients;
using FitSyncHub.IntervalsICU.HttpClients.Models.Requests;
using FitSyncHub.IntervalsICU.Options;
using Microsoft.AspNetCore.WebUtilities;

namespace FitSyncHub.IntervalsIcu.UnitTests;

public sealed class IntervalsIcuActivityProjectionUnitTest
{
    private static readonly ActivityListQueryParams Query =
        new(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 17));

    [Fact]
    public async Task ListActivities_WithSelector_UsesWireNamesAndKeepsStravaPlaceholder()
    {
        using var handler = new ActivityListHandler("""
            [
                {"source":"STRAVA"},
                {"source":"UPLOAD","id":"i123","start_date":"2026-11-17T08:00:00Z","HSIh":7.5}
            ]
            """);
        using var httpClient = CreateHttpClient(handler);
        var client = CreateClient(httpClient);

        var result = await client.ListActivities(Query,
            activity => new { activity.Id, activity.StartDate, activity.HeatLoad, activity.Tags },
            TestContext.Current.CancellationToken);

        Assert.Collection(result,
            activity => Assert.Null(activity),
            activity =>
            {
                Assert.NotNull(activity);
                Assert.Equal("i123", activity.Id);
                Assert.Equal(new DateTime(2026, 11, 17, 8, 0, 0, DateTimeKind.Utc), activity.StartDate);
                Assert.Equal(7.5, activity.HeatLoad);
                Assert.Null(activity.Tags);
            });
        Assert.Equal(new[] { "HSIh", "id", "source", "start_date", "tags" },
            handler.Fields!.Split(',').Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ListActivities_WithIsRideSelector_RequestsTypeAndComputesIsRide()
    {
        using var handler = new ActivityListHandler("""
            [
                {"source":"UPLOAD","type":"VirtualRide"},
                {"source":"UPLOAD","type":"Run"}
            ]
            """);
        using var httpClient = CreateHttpClient(handler);
        var client = CreateClient(httpClient);

        var result = await client.ListActivities(Query, activity => new { activity.IsRide },
            TestContext.Current.CancellationToken);

        Assert.Collection(result,
            activity => Assert.True(activity!.IsRide),
            activity => Assert.False(activity!.IsRide));
        Assert.Equal(new[] { "source", "type" }, handler.Fields!.Split(',').Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ListActivities_WithSelector_RequiresSelectedNonNullableProperty()
    {
        using var handler = new ActivityListHandler("""[{"source":"UPLOAD","id":"i123"}]""");
        using var httpClient = CreateHttpClient(handler);
        var client = CreateClient(httpClient);

        await Assert.ThrowsAsync<JsonException>(() => client.ListActivities(
            Query, activity => new { activity.Id, activity.StartDate }, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ListActivities_WithoutSelector_StillRequiresFullResponse()
    {
        using var handler = new ActivityListHandler("""[{"source":"UPLOAD","id":"i123"}]""");
        using var httpClient = CreateHttpClient(handler);
        var client = CreateClient(httpClient);

        await Assert.ThrowsAsync<JsonException>(() => client.ListActivities(
            Query, TestContext.Current.CancellationToken));
        Assert.Null(handler.Fields);
    }

    [Fact]
    public async Task ListActivities_WithEndDatesSelector_RequestsEachDependencyOnce()
    {
        using var handler = new ActivityListHandler("""
            [{"source":"UPLOAD","start_date":"2026-11-17T08:00:00Z","start_date_local":"2026-11-17T10:00:00","elapsed_time":90}]
            """);
        using var httpClient = CreateHttpClient(handler);
        var client = CreateClient(httpClient);

        var result = await client.ListActivities(Query, activity => new { activity.EndDate, activity.EndDateLocal },
            TestContext.Current.CancellationToken);

        var activity = Assert.Single(result);
        Assert.NotNull(activity);
        Assert.Equal(new DateTime(2026, 11, 17, 8, 1, 30, DateTimeKind.Utc), activity.EndDate);
        Assert.Equal(new DateTime(2026, 11, 17, 10, 1, 30, DateTimeKind.Unspecified), activity.EndDateLocal);
        Assert.Equal(new[] { "elapsed_time", "source", "start_date", "start_date_local" },
            handler.Fields!.Split(',').Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task ListActivities_WithNestedProperty_RejectsSelectorBeforeRequest()
    {
        using var handler = new ActivityListHandler("[]");
        using var httpClient = CreateHttpClient(handler);
        var client = CreateClient(httpClient);

        var exception = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await client.ListActivities(Query, activity => new { activity.StartDate.Year },
                TestContext.Current.CancellationToken));

        Assert.Equal("selector", exception.ParamName);
        Assert.False(handler.Requested);
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler) =>
        new(handler, disposeHandler: false) { BaseAddress = new Uri("https://example.com/api/v1/") };

    private static IntervalsIcuHttpClient CreateClient(HttpClient httpClient) =>
        new(httpClient, new IntervalsIcuOptions { AthleteId = "0", ApiKey = "" });

    private sealed class ActivityListHandler(string content) : HttpMessageHandler
    {
        public bool Requested { get; private set; }
        public string? Fields { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requested = true;
            var parameters = QueryHelpers.ParseQuery(request.RequestUri!.Query);
            Fields = parameters.TryGetValue("fields", out var fields) ? fields.ToString() : null;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
        }
    }
}
