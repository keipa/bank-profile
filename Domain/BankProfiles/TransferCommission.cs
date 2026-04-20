namespace BankProfiles.Web.Domain.BankProfiles;

public class TransferCommission
{
   public decimal Domestic { get; set; }
   public decimal International { get; set; }
   public string Currency { get; set; } = "USD";
}