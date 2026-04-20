namespace BankProfiles.Web.Domain.BankProfiles;

public class AccountType
{
   public required string Name { get; set; }
   public decimal MinimumBalance { get; set; }
   public decimal MonthlyFee { get; set; }
   public List<string>? Features { get; set; }
}
