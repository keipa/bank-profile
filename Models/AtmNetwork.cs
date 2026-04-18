namespace BankProfiles.Web.Models
{
   public class AtmNetwork
   {
      public int OwnAtms { get; set; }
      public int PartnerAtms { get; set; }
      public bool InternationalAccess { get; set; }
   }
}