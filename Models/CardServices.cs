namespace BankProfiles.Web.Models
{
   public class CardServices
   {
      public decimal DebitCardOpeningFee { get; set; }
      public decimal CreditCardOpeningFee { get; set; }
      public decimal AnnualMaintenanceFee { get; set; }
      public string Currency { get; set; } = "USD";
   }
}