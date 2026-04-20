using BankProfiles.Web.Application.Features.EventSourcing.Models;

namespace BankProfiles.Web.Application.Interfaces.Services.EventSourcing;

public interface IEventMigrationService
{
   Task<MigrationResult> MigrateFromJsonAsync(bool dryRun = false);
   Task<MigrationResult> MigrateSingleBankAsync(string bankCode, bool dryRun = false);
}