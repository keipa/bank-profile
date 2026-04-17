using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Services;

namespace BankProfiles.Tests;

public class EventProjectionServiceTests
{
    private readonly EventProjectionService _sut;

    public EventProjectionServiceTests()
    {
        var factory = TestDbContextFactory.Create();
        _sut = new EventProjectionService(factory, TestDbContextFactory.CreateLogger<EventProjectionService>());
    }

    [Fact]
    public void ProjectFromEvents_ReturnsNull_WhenNoEvents()
    {
        var result = _sut.ProjectFromEvents(new List<MetricEvent>());
        Assert.Null(result);
    }

    [Fact]
    public void ProjectFromEvents_SetsTopLevelStringProperties()
    {
        var events = new List<MetricEvent>
        {
            Evt(1, "bankId", "\"bank-alpha\""),
            Evt(2, "name", "\"Alpha Bank\""),
            Evt(3, "legalName", "\"Alpha Bank LLC\""),
            Evt(4, "status", "\"active\""),
            Evt(5, "headquartersCountry", "\"United States\""),
            Evt(6, "countryOfOwnerResidence", "\"United States\""),
        };

        var profile = _sut.ProjectFromEvents(events)!;

        Assert.Equal("bank-alpha", profile.BankId);
        Assert.Equal("Alpha Bank", profile.Name);
        Assert.Equal("Alpha Bank LLC", profile.LegalName);
        Assert.Equal("active", profile.Status);
        Assert.Equal("United States", profile.HeadquartersCountry);
    }

    [Fact]
    public void ProjectFromEvents_SetsNestedNumericProperties()
    {
        var events = new List<MetricEvent>
        {
            Evt(1, "bankId", "\"bank-alpha\""),
            Evt(2, "ratings.overall", "4.5"),
            Evt(3, "branches.count", "25"),
            Evt(4, "clients.total", "50000"),
        };

        var profile = _sut.ProjectFromEvents(events)!;

        Assert.Equal(4.5, profile.Ratings.Overall);
        Assert.Equal(25, profile.Branches.Count);
        Assert.Equal(50000, profile.Clients.Total);
    }

    [Fact]
    public void ProjectFromEvents_SetsNestedBooleanProperties()
    {
        var events = new List<MetricEvent>
        {
            Evt(1, "bankId", "\"bank-alpha\""),
            Evt(2, "systems.swiftAvailable", "true", "Boolean"),
            Evt(3, "systems.ibanSupported", "false", "Boolean"),
            Evt(4, "systems.sepaAvailable", "true", "Boolean"),
            Evt(5, "digitalChannels.mobileApp", "true", "Boolean"),
        };

        var profile = _sut.ProjectFromEvents(events)!;

        Assert.True(profile.Systems.SwiftAvailable);
        Assert.False(profile.Systems.IbanSupported);
        Assert.True(profile.Systems.SepaAvailable);
        Assert.True(profile.DigitalChannels.MobileApp);
    }

    [Fact]
    public void ProjectFromEvents_SetsListProperties()
    {
        var events = new List<MetricEvent>
        {
            Evt(1, "bankId", "\"bank-alpha\""),
            Evt(2, "systems.cardSystems", "[\"visa\",\"mastercard\"]", "List"),
            Evt(3, "currencies.available", "[\"USD\",\"EUR\",\"GBP\"]", "List"),
        };

        var profile = _sut.ProjectFromEvents(events)!;

        Assert.Equal(new List<string> { "visa", "mastercard" }, profile.Systems.CardSystems);
        Assert.Equal(new List<string> { "USD", "EUR", "GBP" }, profile.Currencies.Available);
    }

    [Fact]
    public void ProjectFromEvents_SetsDeeplyNestedProperties()
    {
        var events = new List<MetricEvent>
        {
            Evt(1, "bankId", "\"bank-alpha\""),
            Evt(2, "fees.commissions.incomingDomesticPercent", "0.5", "Percentage"),
            Evt(3, "fees.commissions.outgoingInternationalPercent", "1.2", "Percentage"),
            Evt(4, "fees.accountFees.monthlyMaintenance", "9.99", "Currency"),
            Evt(5, "fees.cardFees.premiumCardAnnualFee", "99.0", "Currency"),
        };

        var profile = _sut.ProjectFromEvents(events)!;

        Assert.Equal(0.5, profile.Fees.Commissions.IncomingDomesticPercent);
        Assert.Equal(1.2, profile.Fees.Commissions.OutgoingInternationalPercent);
        Assert.Equal(9.99, profile.Fees.AccountFees.MonthlyMaintenance);
        Assert.Equal(99.0, profile.Fees.CardFees.PremiumCardAnnualFee);
    }

    [Fact]
    public void ProjectFromEvents_LastEventWins()
    {
        var events = new List<MetricEvent>
        {
            Evt(1, "ratings.overall", "3.0"),
            Evt(2, "ratings.overall", "3.5"),
            Evt(3, "ratings.overall", "4.2"),
        };

        var profile = _sut.ProjectFromEvents(events)!;

        Assert.Equal(4.2, profile.Ratings.Overall);
    }

    [Fact]
    public void ProjectFromEvents_SetsComplianceFields()
    {
        var events = new List<MetricEvent>
        {
            Evt(1, "compliance.sanctionsRisk", "\"low\""),
            Evt(2, "compliance.amlStatus", "\"good\""),
            Evt(3, "compliance.kycStatus", "\"complete\""),
            Evt(4, "compliance.governmentAffiliate", "false", "Boolean"),
            Evt(5, "compliance.depositInsurance", "true", "Boolean"),
        };

        var profile = _sut.ProjectFromEvents(events)!;

        Assert.Equal("low", profile.Compliance.SanctionsRisk);
        Assert.Equal("good", profile.Compliance.AmlStatus);
        Assert.Equal("complete", profile.Compliance.KycStatus);
        Assert.False(profile.Compliance.GovernmentAffiliate);
        Assert.True(profile.Compliance.DepositInsurance);
    }

    [Fact]
    public void ProjectFromEvents_SetsOptionalNestedObjects()
    {
        var events = new List<MetricEvent>
        {
            Evt(1, "bankId", "\"bank-alpha\""),
            Evt(2, "overview.type", "\"retail bank\""),
            Evt(3, "overview.foundedYear", "1995"),
            Evt(4, "metrics.clientSatisfactionPercent", "85.5", "Percentage"),
            Evt(5, "metrics.openIssues", "3"),
        };

        var profile = _sut.ProjectFromEvents(events)!;

        Assert.NotNull(profile.Overview);
        Assert.Equal("retail bank", profile.Overview!.Type);
        Assert.Equal(1995, profile.Overview.FoundedYear);
        Assert.NotNull(profile.Metrics);
        Assert.Equal(85.5, profile.Metrics!.ClientSatisfactionPercent);
        Assert.Equal(3, profile.Metrics.OpenIssues);
    }

    [Fact]
    public void ProjectFromEvents_IgnoresUnknownMetricNames()
    {
        var events = new List<MetricEvent>
        {
            Evt(1, "bankId", "\"bank-alpha\""),
            Evt(2, "nonExistent.field", "\"value\""),
            Evt(3, "ratings.overall", "4.0"),
        };

        var profile = _sut.ProjectFromEvents(events)!;

        // Should not throw; known fields still populated
        Assert.Equal(4.0, profile.Ratings.Overall);
    }

    private static MetricEvent Evt(long seq, string name, string value, string type = "Text")
    {
        return new MetricEvent
        {
            EventId = seq,
            BankCode = "bank-alpha",
            Country = "US",
            MetricName = name,
            MetricValue = value,
            MetricType = type,
            EventSequence = seq,
            CreatedDate = DateTime.UtcNow,
            EventVersion = 1
        };
    }
}
