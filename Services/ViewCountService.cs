using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankProfiles.Web.Services;

public interface IViewCountService
{
    Task IncrementViewCountAsync(string bankCode);
    Task<long> GetViewCountAsync(string bankCode);
    Task<List<(string BankCode, long ViewCount)>> GetMostViewedBanksAsync(int topN = 10);
    Task RecordViewHistorySnapshotAsync(string bankCode);
}

public class ViewCountService : IViewCountService
{
    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly ILogger<ViewCountService> _logger;

    public ViewCountService(IDbContextFactory<BankDbContext> contextFactory, ILogger<ViewCountService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task IncrementViewCountAsync(string bankCode)
    {
        if (!ValidationHelper.IsValidBankCode(bankCode))
        {
            _logger.LogWarning("Invalid bank code attempted: {BankCode}", bankCode);
            return;
        }

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var bank = await context.Banks
                .FirstOrDefaultAsync(b => b.BankCode == bankCode);

            if (bank == null)
            {
                // Create bank entry if it doesn't exist
                bank = new Bank
                {
                    BankCode = bankCode,
                    ViewCount = 0,
                    CreatedDate = DateTime.UtcNow
                };
                context.Banks.Add(bank);
            }

            bank.ViewCount++;
            bank.LastViewedDate = DateTime.UtcNow;

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for {BankCode}", bankCode);
        }
    }

    public async Task<long> GetViewCountAsync(string bankCode)
    {
        if (!ValidationHelper.IsValidBankCode(bankCode))
        {
            _logger.LogWarning("Invalid bank code attempted: {BankCode}", bankCode);
            return 0;
        }

        using var context = await _contextFactory.CreateDbContextAsync();
        
        var bank = await context.Banks
            .FirstOrDefaultAsync(b => b.BankCode == bankCode);

        return bank?.ViewCount ?? 0;
    }

    public async Task<List<(string BankCode, long ViewCount)>> GetMostViewedBanksAsync(int topN = 10)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var topBanks = await context.Banks
            .OrderByDescending(b => b.ViewCount)
            .Take(topN)
            .Select(b => new { b.BankCode, b.ViewCount })
            .ToListAsync();

        return topBanks.Select(b => (b.BankCode, b.ViewCount)).ToList();
    }

    public async Task RecordViewHistorySnapshotAsync(string bankCode)
    {
        if (!ValidationHelper.IsValidBankCode(bankCode))
        {
            _logger.LogWarning("Invalid bank code attempted: {BankCode}", bankCode);
            return;
        }

        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            var bank = await context.Banks
                .FirstOrDefaultAsync(b => b.BankCode == bankCode);

            if (bank == null)
            {
                _logger.LogWarning("Bank {BankCode} not found for view history snapshot", bankCode);
                return;
            }

            var viewHistory = new ViewHistory
            {
                BankId = bank.BankId,
                ViewCount = bank.ViewCount,  // No cast needed - both are long now
                RecordedDate = DateTime.UtcNow
            };

            context.ViewHistory.Add(viewHistory);
            await context.SaveChangesAsync();

            _logger.LogInformation("Recorded view history snapshot for {BankCode} with count {ViewCount}", 
                bankCode, bank.ViewCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording view history snapshot for {BankCode}", bankCode);
        }
    }
}
