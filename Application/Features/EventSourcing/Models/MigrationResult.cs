namespace BankProfiles.Web.Application.Features.EventSourcing.Models;

public class MigrationResult
{
   public int BanksProcessed { get; set; }
   public int BanksSkipped { get; set; }
   public int EventsCreated { get; set; }
   public List<string> Errors { get; set; } = new();
   public bool DryRun { get; set; }
}
