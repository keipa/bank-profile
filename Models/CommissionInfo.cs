namespace BankProfiles.Web.Models
{
   public class CommissionInfo
   {
      public TransferCommission? IncomingTransfer { get; set; }
      public TransferCommission? OutgoingTransfer { get; set; }
   }
}