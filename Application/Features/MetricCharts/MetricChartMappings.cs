namespace BankProfiles.Web.Application.Features.MetricCharts;

public static class MetricChartMappings
{
    public static readonly IReadOnlyDictionary<string, string> KeyToEventName =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Overview — Numeric
            ["founded"] = "overview.foundedYear",
            ["overallRating"] = "ratings.overall",
            ["openIssues"] = "metrics.openIssues",
            ["resolvedEvents"] = "metrics.resolvedEvents",
            ["totalClients"] = "clients.total",
            ["retailClients"] = "clients.retail",
            ["businessClients"] = "clients.business",
            ["corporateClients"] = "clients.corporate",
            ["privateBankingClients"] = "clients.privateBanking",

            // Overview — Percentage
            ["clientSatisfaction"] = "metrics.clientSatisfactionPercent",
            ["corporateSatisfaction"] = "metrics.corporateSatisfactionPercent",

            // Systems/Currencies — Percentage
            ["fxMarkup"] = "currencies.fxMarkupPercent",

            // Systems/Branches — Numeric
            ["physicalBranches"] = "branches.count",
            ["atmCount"] = "branches.atmCount",
            ["partnerAtmNetwork"] = "branches.partnerAtmNetwork",

            // Fees — Percentage (Commissions)
            ["incomingDomesticFee"] = "fees.commissions.incomingDomesticPercent",
            ["incomingInternationalFee"] = "fees.commissions.incomingInternationalPercent",
            ["outgoingDomesticFee"] = "fees.commissions.outgoingDomesticPercent",
            ["outgoingInternationalFee"] = "fees.commissions.outgoingInternationalPercent",
            ["atmLocalWithdrawal"] = "fees.commissions.cashWithdrawalLocalAtmPercent",
            ["atmInternationalWithdrawal"] = "fees.commissions.cashWithdrawalInternationalAtmPercent",
            ["commissionFxMarkup"] = "fees.commissions.fxMarkupPercent",

            // Fees — Currency (Account)
            ["accountOpeningFee"] = "fees.accountFees.accountOpening",
            ["monthlyMaintenanceFee"] = "fees.accountFees.monthlyMaintenance",
            ["accountClosureFee"] = "fees.accountFees.accountClosure",
            ["dormancyFee"] = "fees.accountFees.dormancyFee",

            // Fees — Numeric (Account)
            ["dormancyAfterMonths"] = "fees.accountFees.dormancyAfterMonths",

            // Fees — Currency (Card)
            ["cardIssuanceFee"] = "fees.cardFees.cardIssuance",
            ["premiumCardAnnualFee"] = "fees.cardFees.premiumCardAnnualFee",
            ["replacementCardFee"] = "fees.cardFees.replacementCardFee",

            // Fees — Currency (Transfer)
            ["swiftPaymentProcessingFee"] = "fees.transferFees.swiftPaymentProcessing",
            ["urgentPaymentSurcharge"] = "fees.transferFees.urgentPaymentSurcharge",
            ["chargebackHandlingFee"] = "fees.transferFees.chargebackHandling",

            // Compliance — Percentage/Numeric
            ["complaintRatio"] = "metrics.complaintRatioPercent",
            ["avgRemediationDays"] = "metrics.avgRemediationDays",

            // Digital — Percentage/Numeric
            ["uptimePercent"] = "digitalChannels.uptimePercent",
            ["averageAccountOpeningMinutes"] = "digitalChannels.averageAccountOpeningMinutes",
            ["avgResponseTimeMinutes"] = "support.averageResponseTimeMinutes",
        };

    public static readonly IReadOnlySet<string> EventMetricNames =
        new HashSet<string>(KeyToEventName.Values, StringComparer.OrdinalIgnoreCase);
}
