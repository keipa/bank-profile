namespace BankProfiles.Web.Models
{
   public class AccountType
   {
      public required string Name { get; set; }
      public decimal MinimumBalance { get; set; }
      public decimal MonthlyFee { get; set; }
      public List<string>? Features { get; set; }
   }
}