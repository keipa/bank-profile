namespace BankProfiles.Web.Models
{
   public class PremiumBanking
   {
      public bool Available { get; set; }
      public decimal MinimumBalance { get; set; }
      public decimal MonthlyFee { get; set; }
      public List<string>? Benefits { get; set; }
   }
}