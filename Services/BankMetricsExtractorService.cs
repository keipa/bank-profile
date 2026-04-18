using BankProfiles.Web.Models;

namespace BankProfiles.Web.Services;

public class BankMetricsExtractorService : IBankMetricsExtractorService
{
    public const string OverviewRatingsSectionKey = "overviewRatings";
    public const string SystemsCurrenciesSectionKey = "systemsCurrencies";
    public const string FeesCommissionsSectionKey = "feesCommissions";
    public const string ComplianceRiskSectionKey = "complianceRisk";
    public const string DigitalSupportSectionKey = "digitalSupport";

    private readonly ILocalizationService _localization;

    public BankMetricsExtractorService(ILocalizationService localization)
    {
        _localization = localization;
    }

    public Dictionary<string, List<MetricDto>> ExtractMetrics(BankProfile bank)
    {
        return new Dictionary<string, List<MetricDto>>
        {
            [OverviewRatingsSectionKey] = ExtractOverviewMetrics(bank),
            [SystemsCurrenciesSectionKey] = ExtractSystemsMetrics(bank),
            [FeesCommissionsSectionKey] = ExtractFeesMetrics(bank),
            [ComplianceRiskSectionKey] = ExtractComplianceMetrics(bank),
            [DigitalSupportSectionKey] = ExtractDigitalMetrics(bank)
        };
    }

    private MetricDto CreateMetric(string key, object value, MetricType type, string icon, string? unit = null)
    {
        return new MetricDto
        {
            Key = key,
            Label = _localization.GetString($"metric.label.{key}"),
            Value = value,
            Type = type,
            Icon = icon,
            Unit = unit
        };
    }
    
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
        
        // Total Clients
        metrics.Add(CreateMetric(
            key: "totalClients",
            value: bank.Clients.Total,
            type: MetricType.Numeric,
            icon: "fas fa-users"));
        
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
        
        // Account Fees
        if (bank.Fees.AccountFees.MonthlyMaintenance != null)
        {
            metrics.Add(CreateMetric(
                key: "monthlyMaintenanceFee",
                value: bank.Fees.AccountFees.MonthlyMaintenance.Value,
                type: MetricType.Currency,
                icon: "fas fa-calendar-alt"));
        }
        
        if (bank.Fees.CardFees.PremiumCardAnnualFee != null)
        {
            metrics.Add(CreateMetric(
                key: "premiumCardAnnualFee",
                value: bank.Fees.CardFees.PremiumCardAnnualFee.Value,
                type: MetricType.Currency,
                icon: "fas fa-credit-card"));
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
        }
        
        return metrics;
    }
}
