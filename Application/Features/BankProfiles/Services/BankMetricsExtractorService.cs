using BankProfiles.Web.Application.Interfaces.Services.BankProfiles;
using BankProfiles.Web.Application.Interfaces.Services.Localization;
using BankProfiles.Web.Domain.BankProfiles;
using BankProfiles.Web.Domain.Common.Metrics;

namespace BankProfiles.Web.Application.Features.BankProfiles.Services;

public class BankMetricsExtractorService : IBankMetricsExtractorService
{
    public const string OverviewRatingsSectionKey = "overviewRatings";
    public const string SystemsCurrenciesSectionKey = "systemsCurrencies";
    public const string FeesCommissionsSectionKey = "feesCommissions";
    public const string ComplianceRiskSectionKey = "complianceRisk";
    public const string DigitalSupportSectionKey = "digitalSupport";
    public const string ProductsServicesSectionKey = "productsServices";

    private readonly ILocalizationService _localization;

    public BankMetricsExtractorService(ILocalizationService localization) => _localization = localization;

    public Dictionary<string, List<MetricDto>> ExtractMetrics(BankProfile bank) =>
       new()
       {
           [OverviewRatingsSectionKey] = ExtractOverviewMetrics(bank),
           [SystemsCurrenciesSectionKey] = ExtractSystemsMetrics(bank),
           [FeesCommissionsSectionKey] = ExtractFeesMetrics(bank),
           [ComplianceRiskSectionKey] = ExtractComplianceMetrics(bank),
           [DigitalSupportSectionKey] = ExtractDigitalMetrics(bank),
           [ProductsServicesSectionKey] = ExtractProductMetrics(bank)
        };

    private MetricDto CreateMetric(string key, object value, MetricType type, string icon, string? unit = null) =>
       new()
       {
          Key = key,
          Label = _localization.GetString($"metric.label.{key}"),
          Value = value,
          Type = type,
          Icon = icon,
          Unit = unit
       };

    private List<MetricDto> ExtractOverviewMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();

        // Status
        metrics.Add(CreateMetric(
            key: "bankStatus",
            value: bank.Status.Equals("active", StringComparison.OrdinalIgnoreCase),
            type: MetricType.Boolean,
            icon: "fas fa-check-circle"));

        // Founded Year
        if (bank.Overview?.FoundedYear != null)
        {
            metrics.Add(CreateMetric(
                key: "founded",
                value: bank.Overview.FoundedYear.Value,
                type: MetricType.Numeric,
                icon: "fas fa-calendar"));
        }

        // Bank Type
        if (!string.IsNullOrEmpty(bank.Overview?.Type))
        {
            metrics.Add(CreateMetric(
                key: "bankType",
                value: bank.Overview.Type,
                type: MetricType.Text,
                icon: "fas fa-building"));
        }

        // Segment
        if (!string.IsNullOrWhiteSpace(bank.Overview?.Segment))
        {
            metrics.Add(CreateMetric(
                key: "marketSegment",
                value: bank.Overview.Segment,
                type: MetricType.Text,
                icon: "fas fa-layer-group"));
        }

        // Overall Rating
        metrics.Add(CreateMetric(
            key: "overallRating",
            value: bank.Ratings.Overall,
            type: MetricType.Numeric,
            icon: "fas fa-star",
            unit: "/5"));

        // Client Satisfaction
        if (bank.Metrics?.ClientSatisfactionPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "clientSatisfaction",
                value: bank.Metrics.ClientSatisfactionPercent,
                type: MetricType.Percentage,
                icon: "fas fa-smile"));
        }

        // Corporate Satisfaction
        if (bank.Metrics?.CorporateSatisfactionPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "corporateSatisfaction",
                value: bank.Metrics.CorporateSatisfactionPercent,
                type: MetricType.Percentage,
                icon: "fas fa-briefcase"));
        }

        // Open Issues
        if (bank.Metrics?.OpenIssues != null)
        {
            metrics.Add(CreateMetric(
                key: "openIssues",
                value: bank.Metrics.OpenIssues,
                type: MetricType.Numeric,
                icon: "fas fa-exclamation-triangle"));
        }

        // Resolved Events
        if (bank.Metrics?.ResolvedEvents != null)
        {
            metrics.Add(CreateMetric(
                key: "resolvedEvents",
                value: bank.Metrics.ResolvedEvents,
                type: MetricType.Numeric,
                icon: "fas fa-check-double"));
        }

        // Total Clients
        metrics.Add(CreateMetric(
            key: "totalClients",
            value: bank.Clients.Total,
            type: MetricType.Numeric,
            icon: "fas fa-users"));

        // Client Segments
        if (bank.Clients.Retail != null)
        {
            metrics.Add(CreateMetric(
                key: "retailClients",
                value: bank.Clients.Retail.Value,
                type: MetricType.Numeric,
                icon: "fas fa-user"));
        }

        if (bank.Clients.Business != null)
        {
            metrics.Add(CreateMetric(
                key: "businessClients",
                value: bank.Clients.Business.Value,
                type: MetricType.Numeric,
                icon: "fas fa-store"));
        }

        if (bank.Clients.Corporate != null)
        {
            metrics.Add(CreateMetric(
                key: "corporateClients",
                value: bank.Clients.Corporate.Value,
                type: MetricType.Numeric,
                icon: "fas fa-building-columns"));
        }

        if (bank.Clients.PrivateBanking != null)
        {
            metrics.Add(CreateMetric(
                key: "privateBankingClients",
                value: bank.Clients.PrivateBanking.Value,
                type: MetricType.Numeric,
                icon: "fas fa-user-tie"));
        }

        return metrics;
    }

    private List<MetricDto> ExtractSystemsMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();

        // SWIFT
        metrics.Add(CreateMetric(
            key: "swiftAvailable",
            value: bank.Systems.SwiftAvailable,
            type: MetricType.Boolean,
            icon: "fas fa-exchange-alt"));

        // IBAN
        metrics.Add(CreateMetric(
            key: "ibanSupported",
            value: bank.Systems.IbanSupported,
            type: MetricType.Boolean,
            icon: "fas fa-barcode"));

        // SEPA
        metrics.Add(CreateMetric(
            key: "sepaAvailable",
            value: bank.Systems.SepaAvailable,
            type: MetricType.Boolean,
            icon: "fas fa-euro-sign"));

        // Local Clearing
        if (bank.Systems.LocalClearing != null)
        {
            metrics.Add(CreateMetric(
                key: "localClearing",
                value: bank.Systems.LocalClearing.Value,
                type: MetricType.Boolean,
                icon: "fas fa-building"));
        }

        // Instant Transfers
        if (bank.Systems.InstantTransfers != null)
        {
            metrics.Add(CreateMetric(
                key: "instantTransfers",
                value: bank.Systems.InstantTransfers.Value,
                type: MetricType.Boolean,
                icon: "fas fa-bolt"));
        }

        // Card Systems
        metrics.Add(CreateMetric(
            key: "cardSystems",
            value: bank.Systems.CardSystems,
            type: MetricType.List,
            icon: "fas fa-credit-card"));

        // Available Currencies
        metrics.Add(CreateMetric(
            key: "availableCurrencies",
            value: bank.Currencies.Available,
            type: MetricType.List,
            icon: "fas fa-coins"));

        // Base Currency
        if (!string.IsNullOrWhiteSpace(bank.Currencies.BaseCurrency))
        {
            metrics.Add(CreateMetric(
                key: "baseCurrency",
                value: bank.Currencies.BaseCurrency,
                type: MetricType.Text,
                icon: "fas fa-money-bill-wave"));
        }

        // Multi-Currency Accounts
        if (bank.Currencies.MultiCurrencyAccounts != null)
        {
            metrics.Add(CreateMetric(
                key: "multiCurrencyAccounts",
                value: bank.Currencies.MultiCurrencyAccounts.Value,
                type: MetricType.Boolean,
                icon: "fas fa-globe"));
        }

        // FX Markup
        if (bank.Currencies.FxMarkupPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "fxMarkup",
                value: bank.Currencies.FxMarkupPercent.Value,
                type: MetricType.Percentage,
                icon: "fas fa-percent"));
        }

        // Crypto Exposure
        if (!string.IsNullOrEmpty(bank.Systems.CryptoExposure))
        {
            metrics.Add(CreateMetric(
                key: "cryptoExposure",
                value: bank.Systems.CryptoExposure,
                type: MetricType.Text,
                icon: "fab fa-bitcoin"));
        }

        // Branches
        metrics.Add(CreateMetric(
            key: "physicalBranches",
            value: bank.Branches.Count,
            type: MetricType.Numeric,
            icon: "fas fa-map-marker-alt"));

        // ATMs
        if (bank.Branches.AtmCount != null)
        {
            metrics.Add(CreateMetric(
                key: "atmCount",
                value: bank.Branches.AtmCount.Value,
                type: MetricType.Numeric,
                icon: "fas fa-credit-card"));
        }

        // Branch Countries
        if (bank.Branches.Countries != null && bank.Branches.Countries.Count > 0)
        {
            metrics.Add(CreateMetric(
                key: "branchCountries",
                value: bank.Branches.Countries,
                type: MetricType.List,
                icon: "fas fa-globe-europe"));
        }

        // Partner ATM Network
        if (bank.Branches.PartnerAtmNetwork != null)
        {
            metrics.Add(CreateMetric(
                key: "partnerAtmNetwork",
                value: bank.Branches.PartnerAtmNetwork.Value,
                type: MetricType.Numeric,
                icon: "fas fa-network-wired"));
        }

        return metrics;
    }

    private List<MetricDto> ExtractFeesMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();

        // Commissions
        if (bank.Fees.Commissions.IncomingDomesticPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "incomingDomesticFee",
                value: bank.Fees.Commissions.IncomingDomesticPercent.Value,
                type: MetricType.Percentage,
                icon: "fas fa-arrow-down"));
        }

        if (bank.Fees.Commissions.IncomingInternationalPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "incomingInternationalFee",
                value: bank.Fees.Commissions.IncomingInternationalPercent.Value,
                type: MetricType.Percentage,
                icon: "fas fa-globe-americas"));
        }

        if (bank.Fees.Commissions.OutgoingDomesticPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "outgoingDomesticFee",
                value: bank.Fees.Commissions.OutgoingDomesticPercent.Value,
                type: MetricType.Percentage,
                icon: "fas fa-arrow-up"));
        }

        if (bank.Fees.Commissions.OutgoingInternationalPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "outgoingInternationalFee",
                value: bank.Fees.Commissions.OutgoingInternationalPercent.Value,
                type: MetricType.Percentage,
                icon: "fas fa-plane"));
        }

        if (bank.Fees.Commissions.CashWithdrawalLocalAtmPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "atmLocalWithdrawal",
                value: bank.Fees.Commissions.CashWithdrawalLocalAtmPercent.Value,
                type: MetricType.Percentage,
                icon: "fas fa-credit-card"));
        }

        if (bank.Fees.Commissions.CashWithdrawalInternationalAtmPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "atmInternationalWithdrawal",
                value: bank.Fees.Commissions.CashWithdrawalInternationalAtmPercent.Value,
                type: MetricType.Percentage,
                icon: "fas fa-credit-card"));
        }

        if (bank.Fees.Commissions.FxMarkupPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "commissionFxMarkup",
                value: bank.Fees.Commissions.FxMarkupPercent.Value,
                type: MetricType.Percentage,
                icon: "fas fa-percent"));
        }

        // Account Fees
        if (bank.Fees.AccountFees.AccountOpening != null)
        {
            metrics.Add(CreateMetric(
                key: "accountOpeningFee",
                value: bank.Fees.AccountFees.AccountOpening.Value,
                type: MetricType.Currency,
                icon: "fas fa-door-open"));
        }

        if (bank.Fees.AccountFees.MonthlyMaintenance != null)
        {
            metrics.Add(CreateMetric(
                key: "monthlyMaintenanceFee",
                value: bank.Fees.AccountFees.MonthlyMaintenance.Value,
                type: MetricType.Currency,
                icon: "fas fa-calendar-alt"));
        }

        if (bank.Fees.AccountFees.AccountClosure != null)
        {
            metrics.Add(CreateMetric(
                key: "accountClosureFee",
                value: bank.Fees.AccountFees.AccountClosure.Value,
                type: MetricType.Currency,
                icon: "fas fa-door-closed"));
        }

        if (bank.Fees.AccountFees.DormancyAfterMonths != null)
        {
            metrics.Add(CreateMetric(
                key: "dormancyAfterMonths",
                value: bank.Fees.AccountFees.DormancyAfterMonths.Value,
                type: MetricType.Numeric,
                icon: "fas fa-hourglass-half"));
        }

        if (bank.Fees.AccountFees.DormancyFee != null)
        {
            metrics.Add(CreateMetric(
                key: "dormancyFee",
                value: bank.Fees.AccountFees.DormancyFee.Value,
                type: MetricType.Currency,
                icon: "fas fa-moon"));
        }

        if (bank.Fees.CardFees.CardIssuance != null)
        {
            metrics.Add(CreateMetric(
                key: "cardIssuanceFee",
                value: bank.Fees.CardFees.CardIssuance.Value,
                type: MetricType.Currency,
                icon: "fas fa-id-card"));
        }

        if (bank.Fees.CardFees.PremiumCardAnnualFee != null)
        {
            metrics.Add(CreateMetric(
                key: "premiumCardAnnualFee",
                value: bank.Fees.CardFees.PremiumCardAnnualFee.Value,
                type: MetricType.Currency,
                icon: "fas fa-credit-card"));
        }

        if (bank.Fees.CardFees.ReplacementCardFee != null)
        {
            metrics.Add(CreateMetric(
                key: "replacementCardFee",
                value: bank.Fees.CardFees.ReplacementCardFee.Value,
                type: MetricType.Currency,
                icon: "fas fa-credit-card"));
        }

        if (bank.Fees.TransferFees.SwiftPaymentProcessing != null)
        {
            metrics.Add(CreateMetric(
                key: "swiftPaymentProcessingFee",
                value: bank.Fees.TransferFees.SwiftPaymentProcessing.Value,
                type: MetricType.Currency,
                icon: "fas fa-paper-plane"));
        }

        if (bank.Fees.TransferFees.UrgentPaymentSurcharge != null)
        {
            metrics.Add(CreateMetric(
                key: "urgentPaymentSurcharge",
                value: bank.Fees.TransferFees.UrgentPaymentSurcharge.Value,
                type: MetricType.Currency,
                icon: "fas fa-bolt"));
        }

        if (bank.Fees.TransferFees.ChargebackHandling != null)
        {
            metrics.Add(CreateMetric(
                key: "chargebackHandlingFee",
                value: bank.Fees.TransferFees.ChargebackHandling.Value,
                type: MetricType.Currency,
                icon: "fas fa-undo"));
        }

        return metrics;
    }

    private List<MetricDto> ExtractComplianceMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();

        // Sanctions Risk
        metrics.Add(CreateMetric(
            key: "sanctionsRisk",
            value: bank.Compliance.SanctionsRisk,
            type: MetricType.Text,
            icon: "fas fa-shield-alt"));

        // Government Affiliate
        if (bank.Compliance.GovernmentAffiliate != null)
        {
            metrics.Add(CreateMetric(
                key: "governmentAffiliate",
                value: bank.Compliance.GovernmentAffiliate.Value,
                type: MetricType.Boolean,
                icon: "fas fa-landmark"));
        }

        if (!string.IsNullOrWhiteSpace(bank.Compliance.PepExposure))
        {
            metrics.Add(CreateMetric(
                key: "pepExposure",
                value: bank.Compliance.PepExposure,
                type: MetricType.Text,
                icon: "fas fa-user-shield"));
        }

        if (!string.IsNullOrWhiteSpace(bank.Compliance.OffshoreLinks))
        {
            metrics.Add(CreateMetric(
                key: "offshoreLinks",
                value: bank.Compliance.OffshoreLinks,
                type: MetricType.Text,
                icon: "fas fa-anchor"));
        }

        metrics.Add(CreateMetric(
            key: "amlStatus",
            value: bank.Compliance.AmlStatus,
            type: MetricType.Text,
            icon: "fas fa-clipboard-check"));

        metrics.Add(CreateMetric(
            key: "kycStatus",
            value: bank.Compliance.KycStatus,
            type: MetricType.Text,
            icon: "fas fa-id-badge"));

        if (bank.Compliance.FATCA != null)
        {
            metrics.Add(CreateMetric(
                key: "fatca",
                value: bank.Compliance.FATCA.Value,
                type: MetricType.Boolean,
                icon: "fas fa-gavel"));
        }

        if (bank.Compliance.CRS != null)
        {
            metrics.Add(CreateMetric(
                key: "crs",
                value: bank.Compliance.CRS.Value,
                type: MetricType.Boolean,
                icon: "fas fa-globe-americas"));
        }

        if (bank.Compliance.AuditPublished != null)
        {
            metrics.Add(CreateMetric(
                key: "auditPublished",
                value: bank.Compliance.AuditPublished.Value,
                type: MetricType.Boolean,
                icon: "fas fa-file-contract"));
        }

        if (bank.Compliance.DepositInsurance != null)
        {
            metrics.Add(CreateMetric(
                key: "depositInsurance",
                value: bank.Compliance.DepositInsurance.Value,
                type: MetricType.Boolean,
                icon: "fas fa-shield"));
        }

        // Complaint Ratio
        if (bank.Metrics?.ComplaintRatioPercent != null)
        {
            metrics.Add(CreateMetric(
                key: "complaintRatio",
                value: bank.Metrics.ComplaintRatioPercent,
                type: MetricType.Percentage,
                icon: "fas fa-comment-slash"));
        }

        // Avg Remediation Days
        if (bank.Metrics?.AvgRemediationDays != null)
        {
            metrics.Add(CreateMetric(
                key: "avgRemediationDays",
                value: bank.Metrics.AvgRemediationDays,
                type: MetricType.Numeric,
                icon: "fas fa-clock"));
        }

        // Red Flags
        var redFlagCount = bank.RedFlags?.Count ?? 0;
        metrics.Add(CreateMetric(
            key: "redFlags",
            value: redFlagCount,
            type: MetricType.Numeric,
            icon: "fas fa-flag"));

        return metrics;
    }

    private List<MetricDto> ExtractDigitalMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();

        // Mobile App
        if (bank.DigitalChannels.MobileApp != null)
        {
            metrics.Add(CreateMetric(
                key: "mobileApp",
                value: bank.DigitalChannels.MobileApp.Value,
                type: MetricType.Boolean,
                icon: "fas fa-mobile-alt"));
        }

        // Web Banking
        if (bank.DigitalChannels.WebBanking != null)
        {
            metrics.Add(CreateMetric(
                key: "webBanking",
                value: bank.DigitalChannels.WebBanking.Value,
                type: MetricType.Boolean,
                icon: "fas fa-desktop"));
        }

        // iOS
        if (bank.DigitalChannels.Ios != null)
        {
            metrics.Add(CreateMetric(
                key: "iosApp",
                value: bank.DigitalChannels.Ios.Value,
                type: MetricType.Boolean,
                icon: "fab fa-apple"));
        }

        // Android
        if (bank.DigitalChannels.Android != null)
        {
            metrics.Add(CreateMetric(
                key: "androidApp",
                value: bank.DigitalChannels.Android.Value,
                type: MetricType.Boolean,
                icon: "fab fa-android"));
        }

        // Biometric Login
        if (bank.DigitalChannels.BiometricLogin != null)
        {
            metrics.Add(CreateMetric(
                key: "biometricAuthentication",
                value: bank.DigitalChannels.BiometricLogin.Value,
                type: MetricType.Boolean,
                icon: "fas fa-fingerprint"));
        }

        // API Access
        if (bank.DigitalChannels.ApiAccess != null)
        {
            metrics.Add(CreateMetric(
                key: "apiAccessAvailable",
                value: bank.DigitalChannels.ApiAccess.Value,
                type: MetricType.Boolean,
                icon: "fas fa-code"));
        }

        if (bank.DigitalChannels.DeviceTrust != null)
        {
            metrics.Add(CreateMetric(
                key: "deviceTrust",
                value: bank.DigitalChannels.DeviceTrust.Value,
                type: MetricType.Boolean,
                icon: "fas fa-shield-virus"));
        }

        if (bank.DigitalChannels.PushNotifications != null)
        {
            metrics.Add(CreateMetric(
                key: "pushNotifications",
                value: bank.DigitalChannels.PushNotifications.Value,
                type: MetricType.Boolean,
                icon: "fas fa-bell"));
        }

        if (bank.DigitalChannels.UptimePercent != null)
        {
            metrics.Add(CreateMetric(
                key: "uptimePercent",
                value: bank.DigitalChannels.UptimePercent.Value,
                type: MetricType.Percentage,
                icon: "fas fa-server"));
        }

        if (bank.DigitalChannels.AverageAccountOpeningMinutes != null)
        {
            metrics.Add(CreateMetric(
                key: "averageAccountOpeningMinutes",
                value: (int)bank.DigitalChannels.AverageAccountOpeningMinutes.Value,
                type: MetricType.Numeric,
                icon: "fas fa-stopwatch-20"));
        }

        // Support Channels
        if (bank.Support != null)
        {
            if (bank.Support.Available24x7 != null)
            {
                metrics.Add(CreateMetric(
                    key: "support24x7",
                    value: bank.Support.Available24x7.Value,
                    type: MetricType.Boolean,
                    icon: "fas fa-clock"));
            }

            if (bank.Support.Channels != null && bank.Support.Channels.Count > 0)
            {
                metrics.Add(CreateMetric(
                    key: "supportChannels",
                    value: bank.Support.Channels,
                    type: MetricType.List,
                    icon: "fas fa-headset"));
            }

            if (bank.Support.AverageResponseTimeMinutes != null)
            {
                metrics.Add(CreateMetric(
                    key: "avgResponseTimeMinutes",
                    value: (int)bank.Support.AverageResponseTimeMinutes.Value,
                    type: MetricType.Numeric,
                    icon: "fas fa-stopwatch"));
            }

            if (bank.Support.Languages != null && bank.Support.Languages.Count > 0)
            {
                metrics.Add(CreateMetric(
                    key: "supportLanguages",
                    value: bank.Support.Languages,
                    type: MetricType.List,
                    icon: "fas fa-language"));
            }
        }

        return metrics;
    }

    private List<MetricDto> ExtractProductMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();

        if (bank.Products == null)
        {
            return metrics;
        }

        if (bank.Products.Accounts != null)
        {
            metrics.Add(CreateMetric("productsAccounts", bank.Products.Accounts.Value, MetricType.Boolean, "fas fa-wallet"));
        }

        if (bank.Products.Cards != null)
        {
            metrics.Add(CreateMetric("productsCards", bank.Products.Cards.Value, MetricType.Boolean, "fas fa-credit-card"));
        }

        if (bank.Products.Savings != null)
        {
            metrics.Add(CreateMetric("productsSavings", bank.Products.Savings.Value, MetricType.Boolean, "fas fa-piggy-bank"));
        }

        if (bank.Products.Loans != null)
        {
            metrics.Add(CreateMetric("productsLoans", bank.Products.Loans.Value, MetricType.Boolean, "fas fa-hand-holding-usd"));
        }

        if (bank.Products.Mortgages != null)
        {
            metrics.Add(CreateMetric("productsMortgages", bank.Products.Mortgages.Value, MetricType.Boolean, "fas fa-home"));
        }

        if (bank.Products.InvestmentTools != null)
        {
            metrics.Add(CreateMetric("productsInvestmentTools", bank.Products.InvestmentTools.Value, MetricType.Boolean, "fas fa-chart-line"));
        }

        if (bank.Products.MerchantAcquiring != null)
        {
            metrics.Add(CreateMetric("productsMerchantAcquiring", bank.Products.MerchantAcquiring.Value, MetricType.Boolean, "fas fa-store"));
        }

        if (bank.Products.Payroll != null)
        {
            metrics.Add(CreateMetric("productsPayroll", bank.Products.Payroll.Value, MetricType.Boolean, "fas fa-money-check-alt"));
        }

        if (bank.Products.Escrow != null)
        {
            metrics.Add(CreateMetric("productsEscrow", bank.Products.Escrow.Value, MetricType.Boolean, "fas fa-lock"));
        }

        if (bank.Products.TradeFinance != null)
        {
            metrics.Add(CreateMetric("productsTradeFinance", bank.Products.TradeFinance.Value, MetricType.Boolean, "fas fa-ship"));
        }

        return metrics;
    }
}
