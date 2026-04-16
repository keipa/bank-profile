using BankProfiles.Web.Models;

namespace BankProfiles.Web.Services;

public interface IBankMetricsExtractorService
{
    Dictionary<string, List<MetricDto>> ExtractMetrics(BankProfile bank);
}

public class BankMetricsExtractorService : IBankMetricsExtractorService
{
    public Dictionary<string, List<MetricDto>> ExtractMetrics(BankProfile bank)
    {
        var metrics = new Dictionary<string, List<MetricDto>>
        {
            ["Overview & Ratings"] = ExtractOverviewMetrics(bank),
            ["Systems & Currencies"] = ExtractSystemsMetrics(bank),
            ["Fees & Commissions"] = ExtractFeesMetrics(bank),
            ["Compliance & Risk"] = ExtractComplianceMetrics(bank),
            ["Digital & Support"] = ExtractDigitalMetrics(bank)
        };
        
        return metrics;
    }
    
    private List<MetricDto> ExtractOverviewMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();
        
        // Status
        metrics.Add(new MetricDto
        {
            Label = "Bank Status",
            Value = bank.Status.Equals("active", StringComparison.OrdinalIgnoreCase),
            Type = MetricType.Boolean,
            Icon = "fas fa-check-circle"
        });
        
        // Founded Year
        if (bank.Overview?.FoundedYear != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Founded",
                Value = bank.Overview.FoundedYear.Value,
                Type = MetricType.Numeric,
                Icon = "fas fa-calendar"
            });
        }
        
        // Bank Type
        if (!string.IsNullOrEmpty(bank.Overview?.Type))
        {
            metrics.Add(new MetricDto
            {
                Label = "Bank Type",
                Value = bank.Overview.Type,
                Type = MetricType.Text,
                Icon = "fas fa-building"
            });
        }
        
        // Overall Rating
        metrics.Add(new MetricDto
        {
            Label = "Overall Rating",
            Value = bank.Ratings.Overall,
            Type = MetricType.Numeric,
            Unit = "/5",
            Icon = "fas fa-star"
        });
        
        // Client Satisfaction
        if (bank.Metrics?.ClientSatisfactionPercent != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Client Satisfaction",
                Value = bank.Metrics.ClientSatisfactionPercent,
                Type = MetricType.Percentage,
                Icon = "fas fa-smile"
            });
        }
        
        // Corporate Satisfaction
        if (bank.Metrics?.CorporateSatisfactionPercent != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Corporate Satisfaction",
                Value = bank.Metrics.CorporateSatisfactionPercent,
                Type = MetricType.Percentage,
                Icon = "fas fa-briefcase"
            });
        }
        
        // Open Issues
        if (bank.Metrics?.OpenIssues != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Open Issues",
                Value = bank.Metrics.OpenIssues,
                Type = MetricType.Numeric,
                Icon = "fas fa-exclamation-triangle"
            });
        }
        
        // Total Clients
        metrics.Add(new MetricDto
        {
            Label = "Total Clients",
            Value = bank.Clients.Total,
            Type = MetricType.Numeric,
            Icon = "fas fa-users"
        });
        
        return metrics;
    }
    
    private List<MetricDto> ExtractSystemsMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();
        
        // SWIFT
        metrics.Add(new MetricDto
        {
            Label = "SWIFT Available",
            Value = bank.Systems.SwiftAvailable,
            Type = MetricType.Boolean,
            Icon = "fas fa-exchange-alt"
        });
        
        // IBAN
        metrics.Add(new MetricDto
        {
            Label = "IBAN Supported",
            Value = bank.Systems.IbanSupported,
            Type = MetricType.Boolean,
            Icon = "fas fa-barcode"
        });
        
        // SEPA
        metrics.Add(new MetricDto
        {
            Label = "SEPA Available",
            Value = bank.Systems.SepaAvailable,
            Type = MetricType.Boolean,
            Icon = "fas fa-euro-sign"
        });
        
        // Local Clearing
        if (bank.Systems.LocalClearing != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Local Clearing",
                Value = bank.Systems.LocalClearing.Value,
                Type = MetricType.Boolean,
                Icon = "fas fa-building"
            });
        }
        
        // Instant Transfers
        if (bank.Systems.InstantTransfers != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Instant Transfers",
                Value = bank.Systems.InstantTransfers.Value,
                Type = MetricType.Boolean,
                Icon = "fas fa-bolt"
            });
        }
        
        // Card Systems
        metrics.Add(new MetricDto
        {
            Label = "Card Systems",
            Value = bank.Systems.CardSystems,
            Type = MetricType.List,
            Icon = "fas fa-credit-card"
        });
        
        // Available Currencies
        metrics.Add(new MetricDto
        {
            Label = "Available Currencies",
            Value = bank.Currencies.Available,
            Type = MetricType.List,
            Icon = "fas fa-coins"
        });
        
        // Multi-Currency Accounts
        if (bank.Currencies.MultiCurrencyAccounts != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Multi-Currency Accounts",
                Value = bank.Currencies.MultiCurrencyAccounts.Value,
                Type = MetricType.Boolean,
                Icon = "fas fa-globe"
            });
        }
        
        // FX Markup
        if (bank.Currencies.FxMarkupPercent != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "FX Markup",
                Value = bank.Currencies.FxMarkupPercent.Value,
                Type = MetricType.Percentage,
                Icon = "fas fa-percent"
            });
        }
        
        // Crypto Exposure
        if (!string.IsNullOrEmpty(bank.Systems.CryptoExposure))
        {
            metrics.Add(new MetricDto
            {
                Label = "Crypto Exposure",
                Value = bank.Systems.CryptoExposure,
                Type = MetricType.Text,
                Icon = "fab fa-bitcoin"
            });
        }
        
        // Branches
        metrics.Add(new MetricDto
        {
            Label = "Physical Branches",
            Value = bank.Branches.Count,
            Type = MetricType.Numeric,
            Icon = "fas fa-map-marker-alt"
        });
        
        // ATMs
        if (bank.Branches.AtmCount != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "ATM Count",
                Value = bank.Branches.AtmCount.Value,
                Type = MetricType.Numeric,
                Icon = "fas fa-credit-card"
            });
        }
        
        return metrics;
    }
    
    private List<MetricDto> ExtractFeesMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();
        
        // Commissions
        if (bank.Fees.Commissions.IncomingDomesticPercent != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Incoming Domestic Fee",
                Value = bank.Fees.Commissions.IncomingDomesticPercent.Value,
                Type = MetricType.Percentage,
                Icon = "fas fa-arrow-down"
            });
        }
        
        if (bank.Fees.Commissions.IncomingInternationalPercent != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Incoming International Fee",
                Value = bank.Fees.Commissions.IncomingInternationalPercent.Value,
                Type = MetricType.Percentage,
                Icon = "fas fa-globe-americas"
            });
        }
        
        if (bank.Fees.Commissions.OutgoingDomesticPercent != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Outgoing Domestic Fee",
                Value = bank.Fees.Commissions.OutgoingDomesticPercent.Value,
                Type = MetricType.Percentage,
                Icon = "fas fa-arrow-up"
            });
        }
        
        if (bank.Fees.Commissions.OutgoingInternationalPercent != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Outgoing International Fee",
                Value = bank.Fees.Commissions.OutgoingInternationalPercent.Value,
                Type = MetricType.Percentage,
                Icon = "fas fa-plane"
            });
        }
        
        if (bank.Fees.Commissions.CashWithdrawalLocalAtmPercent != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "ATM Local Withdrawal",
                Value = bank.Fees.Commissions.CashWithdrawalLocalAtmPercent.Value,
                Type = MetricType.Percentage,
                Icon = "fas fa-credit-card"
            });
        }
        
        if (bank.Fees.Commissions.CashWithdrawalInternationalAtmPercent != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "ATM International Withdrawal",
                Value = bank.Fees.Commissions.CashWithdrawalInternationalAtmPercent.Value,
                Type = MetricType.Percentage,
                Icon = "fas fa-credit-card"
            });
        }
        
        // Account Fees
        if (bank.Fees.AccountFees.MonthlyMaintenance != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Monthly Maintenance Fee",
                Value = bank.Fees.AccountFees.MonthlyMaintenance.Value,
                Type = MetricType.Currency,
                Icon = "fas fa-calendar-alt"
            });
        }
        
        if (bank.Fees.CardFees.PremiumCardAnnualFee != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Premium Card Annual Fee",
                Value = bank.Fees.CardFees.PremiumCardAnnualFee.Value,
                Type = MetricType.Currency,
                Icon = "fas fa-credit-card"
            });
        }
        
        return metrics;
    }
    
    private List<MetricDto> ExtractComplianceMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();
        
        // Sanctions Risk
        metrics.Add(new MetricDto
        {
            Label = "Sanctions Risk",
            Value = bank.Compliance.SanctionsRisk,
            Type = MetricType.Text,
            Icon = "fas fa-shield-alt"
        });
        
        // Government Affiliate
        if (bank.Compliance.GovernmentAffiliate != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Government Affiliate",
                Value = bank.Compliance.GovernmentAffiliate.Value,
                Type = MetricType.Boolean,
                Icon = "fas fa-landmark"
            });
        }
        
        // Complaint Ratio
        if (bank.Metrics?.ComplaintRatioPercent != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Complaint Ratio",
                Value = bank.Metrics.ComplaintRatioPercent,
                Type = MetricType.Percentage,
                Icon = "fas fa-comment-slash"
            });
        }
        
        // Avg Remediation Days
        if (bank.Metrics?.AvgRemediationDays != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Avg Remediation Days",
                Value = bank.Metrics.AvgRemediationDays,
                Type = MetricType.Numeric,
                Icon = "fas fa-clock"
            });
        }
        
        // Red Flags
        var redFlagCount = bank.RedFlags?.Count ?? 0;
        metrics.Add(new MetricDto
        {
            Label = "Red Flags",
            Value = redFlagCount,
            Type = MetricType.Numeric,
            Icon = "fas fa-flag"
        });
        
        return metrics;
    }
    
    private List<MetricDto> ExtractDigitalMetrics(BankProfile bank)
    {
        var metrics = new List<MetricDto>();
        
        // Mobile App
        if (bank.DigitalChannels.MobileApp != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Mobile App",
                Value = bank.DigitalChannels.MobileApp.Value,
                Type = MetricType.Boolean,
                Icon = "fas fa-mobile-alt"
            });
        }
        
        // Web Banking
        if (bank.DigitalChannels.WebBanking != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Web Banking",
                Value = bank.DigitalChannels.WebBanking.Value,
                Type = MetricType.Boolean,
                Icon = "fas fa-desktop"
            });
        }
        
        // iOS
        if (bank.DigitalChannels.Ios != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "iOS App",
                Value = bank.DigitalChannels.Ios.Value,
                Type = MetricType.Boolean,
                Icon = "fab fa-apple"
            });
        }
        
        // Android
        if (bank.DigitalChannels.Android != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Android App",
                Value = bank.DigitalChannels.Android.Value,
                Type = MetricType.Boolean,
                Icon = "fab fa-android"
            });
        }
        
        // Biometric Login
        if (bank.DigitalChannels.BiometricLogin != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "Biometric Authentication",
                Value = bank.DigitalChannels.BiometricLogin.Value,
                Type = MetricType.Boolean,
                Icon = "fas fa-fingerprint"
            });
        }
        
        // API Access
        if (bank.DigitalChannels.ApiAccess != null)
        {
            metrics.Add(new MetricDto
            {
                Label = "API Access Available",
                Value = bank.DigitalChannels.ApiAccess.Value,
                Type = MetricType.Boolean,
                Icon = "fas fa-code"
            });
        }
        
        // Support Channels
        if (bank.Support != null)
        {
            if (bank.Support.Available24x7 != null)
            {
                metrics.Add(new MetricDto
                {
                    Label = "24/7 Support",
                    Value = bank.Support.Available24x7.Value,
                    Type = MetricType.Boolean,
                    Icon = "fas fa-clock"
                });
            }
            
            if (bank.Support.Channels != null && bank.Support.Channels.Count > 0)
            {
                metrics.Add(new MetricDto
                {
                    Label = "Support Channels",
                    Value = bank.Support.Channels,
                    Type = MetricType.List,
                    Icon = "fas fa-headset"
                });
            }
            
            if (bank.Support.AverageResponseTimeMinutes != null)
            {
                metrics.Add(new MetricDto
                {
                    Label = "Avg Response Time (min)",
                    Value = (int)bank.Support.AverageResponseTimeMinutes.Value,
                    Type = MetricType.Numeric,
                    Icon = "fas fa-stopwatch"
                });
            }
        }
        
        return metrics;
    }
}
