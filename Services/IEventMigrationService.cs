namespace BankProfiles.Web.Services
{
   public interface IEventMigrationService
   {
      Task<MigrationResult> MigrateFromJsonAsync(bool dryRun = false);
      Task<MigrationResult> MigrateSingleBankAsync(string bankCode, bool dryRun = false);
   }
}