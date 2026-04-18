namespace BankProfiles.Web.Services;

public static class FeedbackMetricCatalog
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<FeedbackMetricDefinition>> DefinitionsByCategory =
        new Dictionary<string, IReadOnlyList<FeedbackMetricDefinition>>(StringComparer.OrdinalIgnoreCase)
        {
            ["fees"] = new List<FeedbackMetricDefinition>
            {
                new("incomingDomesticPercent", "fees.commissions.incomingDomesticPercent"),
                new("incomingInternationalPercent", "fees.commissions.incomingInternationalPercent"),
                new("outgoingDomesticPercent", "fees.commissions.outgoingDomesticPercent"),
                new("outgoingInternationalPercent", "fees.commissions.outgoingInternationalPercent"),
                new("cashWithdrawalLocalAtmPercent", "fees.commissions.cashWithdrawalLocalAtmPercent"),
                new("cashWithdrawalInternationalAtmPercent", "fees.commissions.cashWithdrawalInternationalAtmPercent"),
                new("fxMarkupPercent", "fees.commissions.fxMarkupPercent"),
                new("accountOpening", "fees.accountFees.accountOpening"),
                new("monthlyMaintenance", "fees.accountFees.monthlyMaintenance"),
                new("accountClosure", "fees.accountFees.accountClosure"),
                new("dormancyFee", "fees.accountFees.dormancyFee"),
                new("cardIssuance", "fees.cardFees.cardIssuance"),
                new("premiumCardAnnualFee", "fees.cardFees.premiumCardAnnualFee"),
                new("replacementCardFee", "fees.cardFees.replacementCardFee"),
                new("swiftPaymentProcessing", "fees.transferFees.swiftPaymentProcessing"),
                new("urgentPaymentSurcharge", "fees.transferFees.urgentPaymentSurcharge"),
                new("chargebackHandling", "fees.transferFees.chargebackHandling")
            },
            ["systems"] = new List<FeedbackMetricDefinition>
            {
                new("cardSystems", "systems.cardSystems"),
                new("swiftAvailable", "systems.swiftAvailable"),
                new("ibanSupported", "systems.ibanSupported"),
                new("sepaAvailable", "systems.sepaAvailable"),
                new("localClearing", "systems.localClearing"),
                new("instantTransfers", "systems.instantTransfers"),
                new("cryptoExposure", "systems.cryptoExposure")
            },
            ["currencies"] = new List<FeedbackMetricDefinition>
            {
                new("available", "currencies.available"),
                new("baseCurrency", "currencies.baseCurrency"),
                new("multiCurrencyAccounts", "currencies.multiCurrencyAccounts"),
                new("fxMarkupPercent", "currencies.fxMarkupPercent")
            },
            ["branches"] = new List<FeedbackMetricDefinition>
            {
                new("count", "branches.count"),
                new("countries", "branches.countries"),
                new("atmCount", "branches.atmCount"),
                new("partnerAtmNetwork", "branches.partnerAtmNetwork")
            },
            ["clients"] = new List<FeedbackMetricDefinition>
            {
                new("total", "clients.total"),
                new("retail", "clients.retail"),
                new("business", "clients.business"),
                new("corporate", "clients.corporate"),
                new("privateBanking", "clients.privateBanking")
            },
            ["compliance"] = new List<FeedbackMetricDefinition>
            {
                new("sanctionsRisk", "compliance.sanctionsRisk"),
                new("governmentAffiliate", "compliance.governmentAffiliate"),
                new("pepExposure", "compliance.pepExposure"),
                new("offshoreLinks", "compliance.offshoreLinks"),
                new("amlStatus", "compliance.amlStatus"),
                new("kycStatus", "compliance.kycStatus"),
                new("fATCA", "compliance.fATCA"),
                new("crs", "compliance.crs"),
                new("auditPublished", "compliance.auditPublished"),
                new("depositInsurance", "compliance.depositInsurance")
            },
            ["digitalChannels"] = new List<FeedbackMetricDefinition>
            {
                new("mobileApp", "digitalChannels.mobileApp"),
                new("webBanking", "digitalChannels.webBanking"),
                new("ios", "digitalChannels.ios"),
                new("android", "digitalChannels.android"),
                new("apiAccess", "digitalChannels.apiAccess"),
                new("biometricLogin", "digitalChannels.biometricLogin"),
                new("deviceTrust", "digitalChannels.deviceTrust"),
                new("pushNotifications", "digitalChannels.pushNotifications"),
                new("uptimePercent", "digitalChannels.uptimePercent"),
                new("averageAccountOpeningMinutes", "digitalChannels.averageAccountOpeningMinutes")
            },
            ["support"] = new List<FeedbackMetricDefinition>
            {
                new("available24x7", "support.available24x7"),
                new("channels", "support.channels"),
                new("averageResponseTimeMinutes", "support.averageResponseTimeMinutes"),
                new("languages", "support.languages")
            },
            ["products"] = new List<FeedbackMetricDefinition>
            {
                new("accounts", "products.accounts"),
                new("cards", "products.cards"),
                new("savings", "products.savings"),
                new("loans", "products.loans"),
                new("mortgages", "products.mortgages"),
                new("investmentTools", "products.investmentTools"),
                new("merchantAcquiring", "products.merchantAcquiring"),
                new("payroll", "products.payroll"),
                new("escrow", "products.escrow"),
                new("tradeFinance", "products.tradeFinance")
            },
            ["metrics"] = new List<FeedbackMetricDefinition>
            {
                new("clientSatisfactionPercent", "metrics.clientSatisfactionPercent"),
                new("corporateSatisfactionPercent", "metrics.corporateSatisfactionPercent"),
                new("complaintRatioPercent", "metrics.complaintRatioPercent"),
                new("avgRemediationDays", "metrics.avgRemediationDays"),
                new("openIssues", "metrics.openIssues"),
                new("resolvedEvents", "metrics.resolvedEvents")
            },
            ["overview"] = new List<FeedbackMetricDefinition>
            {
                new("type", "overview.type"),
                new("segment", "overview.segment"),
                new("foundedYear", "overview.foundedYear"),
                new("description", "overview.description"),
                new("logoUrl", "overview.logoUrl"),
                new("iconUrl", "overview.iconUrl")
            }
        };

    public static IReadOnlyDictionary<string, IReadOnlyList<string>> MetricNamesByCategory { get; } = BuildMetricNamesByCategory();

    public static bool TryResolveMetricPath(string metricCategory, string metricName, out string metricPath)
    {
        metricPath = string.Empty;

        if (string.IsNullOrWhiteSpace(metricCategory) || string.IsNullOrWhiteSpace(metricName))
        {
            return false;
        }

        if (!DefinitionsByCategory.TryGetValue(metricCategory.Trim(), out var definitions))
        {
            return false;
        }

        foreach (var definition in definitions)
        {
            if (string.Equals(definition.Name, metricName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                metricPath = definition.Path;
                return true;
            }
        }

        return false;
    }

    public static IReadOnlyList<string> GetMetricsForCategory(string metricCategory)
    {
        if (string.IsNullOrWhiteSpace(metricCategory))
        {
            return Array.Empty<string>();
        }

        return MetricNamesByCategory.TryGetValue(metricCategory.Trim(), out var names)
            ? names
            : Array.Empty<string>();
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildMetricNamesByCategory()
    {
        var result = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var category in DefinitionsByCategory)
        {
            var names = new List<string>(capacity: category.Value.Count);
            foreach (var definition in category.Value)
            {
                names.Add(definition.Name);
            }

            result[category.Key] = names;
        }

        return result;
    }
}
