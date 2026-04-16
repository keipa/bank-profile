using System.Text.Json.Serialization;

namespace BankProfiles.Web.Models;

public class BankOverview
{
    [JsonPropertyName("type")]
    public string? Type { get; set; } // retail bank, digital bank, commercial bank, private bank
    
    [JsonPropertyName("segment")]
    public string? Segment { get; set; }
    
    [JsonPropertyName("foundedYear")]
    public int? FoundedYear { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("logoUrl")]
    public string? LogoUrl { get; set; }
}

public class BankSystems
{
    [JsonPropertyName("cardSystems")]
    public required List<string> CardSystems { get; set; } // visa, mastercard, amex, unionpay, jcb, diners_club, discover, maestro, mir, other
    
    [JsonPropertyName("swiftAvailable")]
    public required bool SwiftAvailable { get; set; }
    
    [JsonPropertyName("ibanSupported")]
    public required bool IbanSupported { get; set; }
    
    [JsonPropertyName("sepaAvailable")]
    public required bool SepaAvailable { get; set; }
    
    [JsonPropertyName("localClearing")]
    public bool? LocalClearing { get; set; }
    
    [JsonPropertyName("instantTransfers")]
    public bool? InstantTransfers { get; set; }
    
    [JsonPropertyName("cryptoExposure")]
    public string? CryptoExposure { get; set; } // none, low, moderate, high, unknown
}

public class BankCurrencies
{
    [JsonPropertyName("available")]
    public required List<string> Available { get; set; } // ISO 4217 currency codes
    
    [JsonPropertyName("baseCurrency")]
    public string? BaseCurrency { get; set; }
    
    [JsonPropertyName("multiCurrencyAccounts")]
    public bool? MultiCurrencyAccounts { get; set; }
    
    [JsonPropertyName("fxMarkupPercent")]
    public double? FxMarkupPercent { get; set; }
}

public class BankFees
{
    [JsonPropertyName("commissions")]
    public required FeesCommissions Commissions { get; set; }
    
    [JsonPropertyName("accountFees")]
    public required FeesAccount AccountFees { get; set; }
    
    [JsonPropertyName("cardFees")]
    public required FeesCard CardFees { get; set; }
    
    [JsonPropertyName("transferFees")]
    public required FeesTransfer TransferFees { get; set; }
}

public class FeesCommissions
{
    [JsonPropertyName("incomingDomesticPercent")]
    public double? IncomingDomesticPercent { get; set; }
    
    [JsonPropertyName("incomingInternationalPercent")]
    public double? IncomingInternationalPercent { get; set; }
    
    [JsonPropertyName("outgoingDomesticPercent")]
    public double? OutgoingDomesticPercent { get; set; }
    
    [JsonPropertyName("outgoingInternationalPercent")]
    public double? OutgoingInternationalPercent { get; set; }
    
    [JsonPropertyName("cashWithdrawalLocalAtmPercent")]
    public double? CashWithdrawalLocalAtmPercent { get; set; }
    
    [JsonPropertyName("cashWithdrawalInternationalAtmPercent")]
    public double? CashWithdrawalInternationalAtmPercent { get; set; }
    
    [JsonPropertyName("fxMarkupPercent")]
    public double? FxMarkupPercent { get; set; }
}

public class FeesAccount
{
    [JsonPropertyName("accountOpening")]
    public double? AccountOpening { get; set; }
    
    [JsonPropertyName("monthlyMaintenance")]
    public double? MonthlyMaintenance { get; set; }
    
    [JsonPropertyName("accountClosure")]
    public double? AccountClosure { get; set; }
    
    [JsonPropertyName("dormancyAfterMonths")]
    public int? DormancyAfterMonths { get; set; }
    
    [JsonPropertyName("dormancyFee")]
    public double? DormancyFee { get; set; }
}

public class FeesCard
{
    [JsonPropertyName("cardIssuance")]
    public double? CardIssuance { get; set; }
    
    [JsonPropertyName("premiumCardAnnualFee")]
    public double? PremiumCardAnnualFee { get; set; }
    
    [JsonPropertyName("replacementCardFee")]
    public double? ReplacementCardFee { get; set; }
}

public class FeesTransfer
{
    [JsonPropertyName("swiftPaymentProcessing")]
    public double? SwiftPaymentProcessing { get; set; }
    
    [JsonPropertyName("urgentPaymentSurcharge")]
    public double? UrgentPaymentSurcharge { get; set; }
    
    [JsonPropertyName("chargebackHandling")]
    public double? ChargebackHandling { get; set; }
}

public class BankBranches
{
    [JsonPropertyName("count")]
    public required int Count { get; set; }
    
    [JsonPropertyName("countries")]
    public List<string>? Countries { get; set; }
    
    [JsonPropertyName("atmCount")]
    public int? AtmCount { get; set; }
    
    [JsonPropertyName("partnerAtmNetwork")]
    public int? PartnerAtmNetwork { get; set; }
}

public class BankClients
{
    [JsonPropertyName("total")]
    public required int Total { get; set; }
    
    [JsonPropertyName("retail")]
    public int? Retail { get; set; }
    
    [JsonPropertyName("business")]
    public int? Business { get; set; }
    
    [JsonPropertyName("corporate")]
    public int? Corporate { get; set; }
    
    [JsonPropertyName("privateBanking")]
    public int? PrivateBanking { get; set; }
    
    [JsonPropertyName("growthHistory")]
    public List<TimestampedMetric>? GrowthHistory { get; set; }
}

public class BankRatings
{
    [JsonPropertyName("overall")]
    public required double Overall { get; set; } // 0-5
    
    [JsonPropertyName("history")]
    public List<RatingPoint>? History { get; set; }
    
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    
    [JsonPropertyName("lastUpdated")]
    public DateTime? LastUpdated { get; set; }
}

public class BankCompliance
{
    [JsonPropertyName("sanctionsRisk")]
    public required string SanctionsRisk { get; set; } // low, moderate, high, critical, unknown
    
    [JsonPropertyName("governmentAffiliate")]
    public bool? GovernmentAffiliate { get; set; }
    
    [JsonPropertyName("pepExposure")]
    public string? PepExposure { get; set; } // low, moderate, high, unknown
    
    [JsonPropertyName("offshoreLinks")]
    public string? OffshoreLinks { get; set; } // none, limited, moderate, significant, unknown
    
    [JsonPropertyName("amlStatus")]
    public required string AmlStatus { get; set; } // good, watch, review, critical, unknown
    
    [JsonPropertyName("kycStatus")]
    public required string KycStatus { get; set; } // complete, partial, weak, unknown
    
    [JsonPropertyName("fATCA")]
    public bool? FATCA { get; set; }
    
    [JsonPropertyName("crs")]
    public bool? CRS { get; set; }
    
    [JsonPropertyName("auditPublished")]
    public bool? AuditPublished { get; set; }
    
    [JsonPropertyName("depositInsurance")]
    public bool? DepositInsurance { get; set; }
}

public class DigitalChannels
{
    [JsonPropertyName("mobileApp")]
    public bool? MobileApp { get; set; }
    
    [JsonPropertyName("webBanking")]
    public bool? WebBanking { get; set; }
    
    [JsonPropertyName("ios")]
    public bool? Ios { get; set; }
    
    [JsonPropertyName("android")]
    public bool? Android { get; set; }
    
    [JsonPropertyName("apiAccess")]
    public bool? ApiAccess { get; set; }
    
    [JsonPropertyName("biometricLogin")]
    public bool? BiometricLogin { get; set; }
    
    [JsonPropertyName("deviceTrust")]
    public bool? DeviceTrust { get; set; }
    
    [JsonPropertyName("pushNotifications")]
    public bool? PushNotifications { get; set; }
    
    [JsonPropertyName("uptimePercent")]
    public double? UptimePercent { get; set; }
    
    [JsonPropertyName("averageAccountOpeningMinutes")]
    public double? AverageAccountOpeningMinutes { get; set; }
}

public class BankSupport
{
    [JsonPropertyName("available24x7")]
    public bool? Available24x7 { get; set; }
    
    [JsonPropertyName("channels")]
    public List<string>? Channels { get; set; } // phone, email, chat, branch, app, web
    
    [JsonPropertyName("averageResponseTimeMinutes")]
    public double? AverageResponseTimeMinutes { get; set; }
    
    [JsonPropertyName("languages")]
    public List<string>? Languages { get; set; }
}

public class BankProducts
{
    [JsonPropertyName("accounts")]
    public bool? Accounts { get; set; }
    
    [JsonPropertyName("cards")]
    public bool? Cards { get; set; }
    
    [JsonPropertyName("savings")]
    public bool? Savings { get; set; }
    
    [JsonPropertyName("loans")]
    public bool? Loans { get; set; }
    
    [JsonPropertyName("mortgages")]
    public bool? Mortgages { get; set; }
    
    [JsonPropertyName("investmentTools")]
    public bool? InvestmentTools { get; set; }
    
    [JsonPropertyName("merchantAcquiring")]
    public bool? MerchantAcquiring { get; set; }
    
    [JsonPropertyName("payroll")]
    public bool? Payroll { get; set; }
    
    [JsonPropertyName("escrow")]
    public bool? Escrow { get; set; }
    
    [JsonPropertyName("tradeFinance")]
    public bool? TradeFinance { get; set; }
}

public class BankMetrics
{
    [JsonPropertyName("clientSatisfactionPercent")]
    public double? ClientSatisfactionPercent { get; set; }
    
    [JsonPropertyName("corporateSatisfactionPercent")]
    public double? CorporateSatisfactionPercent { get; set; }
    
    [JsonPropertyName("complaintRatioPercent")]
    public double? ComplaintRatioPercent { get; set; }
    
    [JsonPropertyName("avgRemediationDays")]
    public double? AvgRemediationDays { get; set; }
    
    [JsonPropertyName("openIssues")]
    public int? OpenIssues { get; set; }
    
    [JsonPropertyName("resolvedEvents")]
    public int? ResolvedEvents { get; set; }
}

public class BankMetadata
{
    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }
    
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
    
    [JsonPropertyName("lastReviewed")]
    public DateTime? LastReviewed { get; set; }
    
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; } // 0-1
}

public class TimestampedMetric
{
    [JsonPropertyName("date")]
    public required string Date { get; set; } // date format
    
    [JsonPropertyName("value")]
    public required double Value { get; set; }
}

public class RatingPoint
{
    [JsonPropertyName("date")]
    public required string Date { get; set; } // date format
    
    [JsonPropertyName("rating")]
    public required double Rating { get; set; } // 0-5
    
    [JsonPropertyName("note")]
    public string? Note { get; set; }
}

public class RedFlagEvent
{
    [JsonPropertyName("date")]
    public required string Date { get; set; } // date format
    
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("severity")]
    public required string Severity { get; set; } // low, moderate, high, critical
    
    [JsonPropertyName("status")]
    public required string Status { get; set; } // open, monitoring, resolved, closed
    
    [JsonPropertyName("impact")]
    public string? Impact { get; set; } // none, low, moderate, high
}
