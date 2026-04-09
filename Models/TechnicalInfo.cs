namespace BankProfiles.Web.Models;

public class TechnicalInfo
{
    public CommissionInfo? Commissions { get; set; }
    public CardServices? CardServices { get; set; }
    public List<string>? AvailableCurrencies { get; set; }
    public PremiumBanking? PremiumBanking { get; set; }
    public DigitalServices? DigitalServices { get; set; }
    public List<AccountType>? AccountTypes { get; set; }
    public AtmNetwork? AtmNetwork { get; set; }
}

public class CommissionInfo
{
    public TransferCommission? IncomingTransfer { get; set; }
    public TransferCommission? OutgoingTransfer { get; set; }
}

public class TransferCommission
{
    public decimal Domestic { get; set; }
    public decimal International { get; set; }
    public string Currency { get; set; } = "USD";
}

public class CardServices
{
    public decimal DebitCardOpeningFee { get; set; }
    public decimal CreditCardOpeningFee { get; set; }
    public decimal AnnualMaintenanceFee { get; set; }
    public string Currency { get; set; } = "USD";
}

public class PremiumBanking
{
    public bool Available { get; set; }
    public decimal MinimumBalance { get; set; }
    public decimal MonthlyFee { get; set; }
    public List<string>? Benefits { get; set; }
}

public class DigitalServices
{
    public bool MobileBanking { get; set; }
    public bool OnlineTrading { get; set; }
    public bool Cryptocurrency { get; set; }
    public bool RoboAdvisor { get; set; }
}

public class AccountType
{
    public required string Name { get; set; }
    public decimal MinimumBalance { get; set; }
    public decimal MonthlyFee { get; set; }
    public List<string>? Features { get; set; }
}

public class AtmNetwork
{
    public int OwnAtms { get; set; }
    public int PartnerAtms { get; set; }
    public bool InternationalAccess { get; set; }
}

public class ContactInfo
{
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}
