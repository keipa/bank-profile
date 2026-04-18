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