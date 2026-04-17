using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Services;

namespace BankProfiles.Tests;

public class EventStoreServiceTests
{
    private readonly EventStoreService _sut;
    private readonly Microsoft.EntityFrameworkCore.IDbContextFactory<BankProfiles.Web.Data.BankDbContext> _factory;

    public EventStoreServiceTests()
    {
        _factory = TestDbContextFactory.Create();
        _sut = new EventStoreService(_factory, TestDbContextFactory.CreateLogger<EventStoreService>());
    }

    [Fact]
    public async Task AppendEventAsync_AssignsSequenceAndCreatedDate()
    {
        var evt = CreateEvent("bank-alpha", "ratings.overall", "4.5");

        var result = await _sut.AppendEventAsync(evt);

        Assert.Equal(1, result.EventSequence);
        Assert.True(result.CreatedDate > DateTime.MinValue);
        Assert.True(result.EventId > 0);
    }

    [Fact]
    public async Task AppendEventAsync_IncrementsSequencePerBank()
    {
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "ratings.overall", "4.0"));
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "status", "\"active\""));
        var third = await _sut.AppendEventAsync(CreateEvent("bank-alpha", "name", "\"Alpha Bank\""));

        Assert.Equal(3, third.EventSequence);
    }

    [Fact]
    public async Task AppendEventAsync_IndependentSequencePerBank()
    {
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "ratings.overall", "4.0"));
        var betaFirst = await _sut.AppendEventAsync(CreateEvent("bank-beta", "ratings.overall", "3.5"));

        Assert.Equal(1, betaFirst.EventSequence);
    }

    [Fact]
    public async Task AppendEventsAsync_BatchInsertWithCorrectSequences()
    {
        var events = new List<MetricEvent>
        {
            CreateEvent("bank-alpha", "name", "\"Alpha Bank\""),
            CreateEvent("bank-alpha", "status", "\"active\""),
            CreateEvent("bank-alpha", "ratings.overall", "4.2")
        };

        var result = await _sut.AppendEventsAsync(events);

        Assert.Equal(3, result.Count);
        Assert.Equal(1, result[0].EventSequence);
        Assert.Equal(2, result[1].EventSequence);
        Assert.Equal(3, result[2].EventSequence);
    }

    [Fact]
    public async Task GetEventsForBankAsync_ReturnsOrderedEvents()
    {
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "name", "\"Alpha\""));
        await _sut.AppendEventAsync(CreateEvent("bank-beta", "name", "\"Beta\""));
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "status", "\"active\""));

        var events = await _sut.GetEventsForBankAsync("bank-alpha");

        Assert.Equal(2, events.Count);
        Assert.Equal("name", events[0].MetricName);
        Assert.Equal("status", events[1].MetricName);
    }

    [Fact]
    public async Task GetEventsByMetricAsync_FiltersCorrectly()
    {
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "ratings.overall", "3.0"));
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "name", "\"Alpha\""));
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "ratings.overall", "4.5"));

        var events = await _sut.GetEventsByMetricAsync("bank-alpha", "ratings.overall");

        Assert.Equal(2, events.Count);
        Assert.All(events, e => Assert.Equal("ratings.overall", e.MetricName));
    }

    [Fact]
    public async Task GetAllBankCodesAsync_ReturnsDistinctCodes()
    {
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "name", "\"Alpha\""));
        await _sut.AppendEventAsync(CreateEvent("bank-beta", "name", "\"Beta\""));
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "status", "\"active\""));

        var codes = await _sut.GetAllBankCodesAsync();

        Assert.Equal(2, codes.Count);
        Assert.Contains("bank-alpha", codes);
        Assert.Contains("bank-beta", codes);
    }

    [Fact]
    public async Task HasEventsAsync_ReturnsTrueWhenEventsExist()
    {
        Assert.False(await _sut.HasEventsAsync("bank-alpha"));
        await _sut.AppendEventAsync(CreateEvent("bank-alpha", "name", "\"Alpha\""));
        Assert.True(await _sut.HasEventsAsync("bank-alpha"));
    }

    [Fact]
    public async Task GetLatestSequenceAsync_ReturnsCorrectValue()
    {
        Assert.Equal(0, await _sut.GetLatestSequenceAsync("bank-alpha"));

        await _sut.AppendEventsAsync(new List<MetricEvent>
        {
            CreateEvent("bank-alpha", "name", "\"Alpha\""),
            CreateEvent("bank-alpha", "status", "\"active\""),
        });

        Assert.Equal(2, await _sut.GetLatestSequenceAsync("bank-alpha"));
    }

    private static MetricEvent CreateEvent(string bankCode, string metricName, string value, string type = "Text")
    {
        return new MetricEvent
        {
            BankCode = bankCode,
            Country = "US",
            MetricName = metricName,
            MetricValue = value,
            MetricType = type,
            Comment = "test"
        };
    }
}
