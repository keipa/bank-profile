namespace BankProfiles.Web.Domain.BankProfiles;

public class CommissionInfo
{
   public TransferCommission? IncomingTransfer { get; set; }
   public TransferCommission? OutgoingTransfer { get; set; }
}