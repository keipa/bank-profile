namespace BankProfiles.Web.Models
{
   public class TransferCommission
   {
      public decimal Domestic { get; set; }
      public decimal International { get; set; }
      public string Currency { get; set; } = "USD";
   }
}