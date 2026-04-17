using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Models;
using BankProfiles.Web.Services;
using System.Text.Json;

namespace BankProfiles.Tests;

/// <summary>
/// Integration tests: round-trip from JSON → events → projection → verify parity.
/// </summary>
public class EventSourcingIntegrationTests
{
    private readonly EventStoreService _eventStore;
    private readonly EventProjectionService _projection;

    public EventSourcingIntegrationTests()
    {
        var factory = TestDbContextFactory.Create();
        _eventStore = new EventStoreService(factory, TestDbContextFactory.CreateLogger<EventStoreService>());
        _projection = new EventProjectionService(factory, TestDbContextFactory.CreateLogger<EventProjectionService>());
    }

    [Fact]
    public async Task RoundTrip_CoreFieldsSurviveEventSourcingCycle()
    {
        // Create events that represent a complete bank profile
        var events = BuildCompleteBankEvents("bank-test");

        await _eventStore.AppendEventsAsync(events);

        var projected = await _projection.ProjectBankProfileAsync("bank-test");

        Assert.NotNull(projected);
        Assert.Equal("bank-test", projected!.BankId);
        Assert.Equal("Test Bank", projected.Name);
        Assert.Equal("Test Bank LLC", projected.LegalName);
        Assert.Equal("active", projected.Status);
        Assert.Equal("United States", projected.HeadquartersCountry);
        Assert.Equal("United States", projected.CountryOfOwnerResidence);
    }

    [Fact]
    public async Task RoundTrip_NestedObjectsSurvive()
    {
        var events = BuildCompleteBankEvents("bank-nested");
        await _eventStore.AppendEventsAsync(events);

        var projected = await _projection.ProjectBankProfileAsync("bank-nested");

        Assert.NotNull(projected);
        Assert.Equal(4.5, projected!.Ratings.Overall);
        Assert.True(projected.Systems.SwiftAvailable);
        Assert.Contains("USD", projected.Currencies.Available);
        Assert.Equal(0.5, projected.Fees.Commissions.IncomingDomesticPercent);
        Assert.Equal("low", projected.Compliance.SanctionsRisk);
        Assert.Equal(10, projected.Branches.Count);
        Assert.Equal(5000, projected.Clients.Total);
    }

    [Fact]
    public async Task RoundTrip_EventOverwriteProducesLatestState()
    {
        var events = new List<MetricEvent>
        {
            Evt("bank-evolve", 1, "bankId", "\"bank-evolve\""),
            Evt("bank-evolve", 2, "ratings.overall", "3.0"),
            Evt("bank-evolve", 3, "ratings.overall", "3.5"),
            Evt("bank-evolve", 4, "ratings.overall", "4.8"),
            Evt("bank-evolve", 5, "status", "\"active\""),
            Evt("bank-evolve", 6, "status", "\"under_review\""),
        };

        await _eventStore.AppendEventsAsync(events);
        var projected = await _projection.ProjectBankProfileAsync("bank-evolve");

        Assert.Equal(4.8, projected!.Ratings.Overall);
        Assert.Equal("under_review", projected.Status);
    }

    [Fact]
    public async Task RoundTrip_MultipleBanksProjectIndependently()
    {
        await _eventStore.AppendEventsAsync(new List<MetricEvent>
        {
            Evt("bank-a", 1, "bankId", "\"bank-a\""),
            Evt("bank-a", 2, "name", "\"Bank A\""),
            Evt("bank-a", 3, "ratings.overall", "4.0"),
        });

        await _eventStore.AppendEventsAsync(new List<MetricEvent>
        {
            Evt("bank-b", 1, "bankId", "\"bank-b\""),
            Evt("bank-b", 2, "name", "\"Bank B\""),
            Evt("bank-b", 3, "ratings.overall", "3.0"),
        });

        var a = await _projection.ProjectBankProfileAsync("bank-a");
        var b = await _projection.ProjectBankProfileAsync("bank-b");

        Assert.Equal("Bank A", a!.Name);
        Assert.Equal(4.0, a.Ratings.Overall);
        Assert.Equal("Bank B", b!.Name);
        Assert.Equal(3.0, b.Ratings.Overall);
    }

    [Fact]
    public async Task HistoryQuery_ReturnsMetricChangesOverTime()
    {
        await _eventStore.AppendEventsAsync(new List<MetricEvent>
        {
            Evt("bank-hist", 1, "ratings.overall", "3.0"),
            Evt("bank-hist", 2, "name", "\"History Bank\""),
            Evt("bank-hist", 3, "ratings.overall", "3.5"),
            Evt("bank-hist", 4, "ratings.overall", "4.0"),
        });

        var ratingHistory = await _eventStore.GetEventsByMetricAsync("bank-hist", "ratings.overall");

        Assert.Equal(3, ratingHistory.Count);
        Assert.Equal("3.0", ratingHistory[0].MetricValue);
        Assert.Equal("3.5", ratingHistory[1].MetricValue);
        Assert.Equal("4.0", ratingHistory[2].MetricValue);
    }

    private static List<MetricEvent> BuildCompleteBankEvents(string bankCode)
    {
        return new List<MetricEvent>
        {
            Evt(bankCode, 1, "bankId", $"\"{bankCode}\""),
            Evt(bankCode, 2, "name", "\"Test Bank\""),
            Evt(bankCode, 3, "legalName", "\"Test Bank LLC\""),
            Evt(bankCode, 4, "status", "\"active\""),
            Evt(bankCode, 5, "headquartersCountry", "\"United States\""),
            Evt(bankCode, 6, "countryOfOwnerResidence", "\"United States\""),
            // Ratings
            Evt(bankCode, 7, "ratings.overall", "4.5"),
            // Systems
            Evt(bankCode, 8, "systems.swiftAvailable", "true", "Boolean"),
            Evt(bankCode, 9, "systems.ibanSupported", "true", "Boolean"),
            Evt(bankCode, 10, "systems.sepaAvailable", "false", "Boolean"),
            Evt(bankCode, 11, "systems.cardSystems", "[\"visa\",\"mastercard\"]", "List"),
            // Currencies
            Evt(bankCode, 12, "currencies.available", "[\"USD\",\"EUR\"]", "List"),
            Evt(bankCode, 13, "currencies.fxMarkupPercent", "1.5"),
            // Fees
            Evt(bankCode, 14, "fees.commissions.incomingDomesticPercent", "0.5", "Percentage"),
            Evt(bankCode, 15, "fees.commissions.outgoingInternationalPercent", "1.2", "Percentage"),
            Evt(bankCode, 16, "fees.accountFees.monthlyMaintenance", "9.99", "Currency"),
            // Branches & Clients
            Evt(bankCode, 17, "branches.count", "10"),
            Evt(bankCode, 18, "clients.total", "5000"),
            // Compliance
            Evt(bankCode, 19, "compliance.sanctionsRisk", "\"low\""),
            Evt(bankCode, 20, "compliance.amlStatus", "\"good\""),
            Evt(bankCode, 21, "compliance.kycStatus", "\"complete\""),
            // Digital
            Evt(bankCode, 22, "digitalChannels.mobileApp", "true", "Boolean"),
            Evt(bankCode, 23, "digitalChannels.webBanking", "true", "Boolean"),
        };
    }

    private static MetricEvent Evt(string bankCode, long seq, string name, string value, string type = "Text")
    {
        return new MetricEvent
        {
            BankCode = bankCode,
            Country = "US",
            MetricName = name,
            MetricValue = value,
            MetricType = type,
            EventSequence = seq,
            EventVersion = 1
        };
    }
}
