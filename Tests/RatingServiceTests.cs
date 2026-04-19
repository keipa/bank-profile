using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Models;
using BankProfiles.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace BankProfiles.Tests;

public class RatingServiceTests
{
    private readonly IDbContextFactory<BankDbContext> _factory;
    private readonly RatingService _sut;

    public RatingServiceTests()
    {
        _factory = TestDbContextFactory.Create(Guid.NewGuid().ToString());
        _sut = new RatingService(
            _factory,
            TestDbContextFactory.CreateLogger<RatingService>(),
            new StubBankDataService());
    }

    [Fact]
    public async Task AddRatingHistorySnapshotAsync_UsesLatestRatingPerCriterion()
    {
        await using (var seedContext = await _factory.CreateDbContextAsync())
        {
            var bank = new Bank
            {
                BankCode = "bank-alpha",
                ViewCount = 0,
                CreatedDate = DateTime.UtcNow
            };

            var serviceCriteria = new RatingCriteria { Name = "Service", DisplayOrder = 1 };
            var feesCriteria = new RatingCriteria { Name = "Fees", DisplayOrder = 2 };

            seedContext.Banks.Add(bank);
            seedContext.RatingCriterias.AddRange(serviceCriteria, feesCriteria);
            await seedContext.SaveChangesAsync();

            seedContext.BankRatings.AddRange(
                new BankRating
                {
                    BankId = bank.BankId,
                    CriteriaId = serviceCriteria.CriteriaId,
                    RatingValue = 6.5m,
                    RatingDate = DateTime.UtcNow.AddDays(-2)
                },
                new BankRating
                {
                    BankId = bank.BankId,
                    CriteriaId = serviceCriteria.CriteriaId,
                    RatingValue = 8.1m,
                    RatingDate = DateTime.UtcNow.AddDays(-1)
                },
                new BankRating
                {
                    BankId = bank.BankId,
                    CriteriaId = feesCriteria.CriteriaId,
                    RatingValue = 7.4m,
                    RatingDate = DateTime.UtcNow.AddDays(-1)
                });

            await seedContext.SaveChangesAsync();
        }

        await _sut.AddRatingHistorySnapshotAsync();

        await using var verifyContext = await _factory.CreateDbContextAsync();
        var snapshots = await verifyContext.RatingHistories
            .OrderBy(h => h.CriteriaId)
            .ToListAsync();

        Assert.Equal(2, snapshots.Count);
        Assert.Contains(snapshots, h => h.OverallRating == 8.1m);
        Assert.Contains(snapshots, h => h.OverallRating == 7.4m);
    }

    [Fact]
    public async Task AddRatingHistorySnapshotAsync_ThrowsOperationCanceled_WhenTokenIsCancelled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => _sut.AddRatingHistorySnapshotAsync(cts.Token));

        await using var verifyContext = await _factory.CreateDbContextAsync();
        Assert.Empty(await verifyContext.RatingHistories.ToListAsync());
    }

    private sealed class StubBankDataService : IBankDataService
    {
        public Task<BankProfile?> GetBankByCodeAsync(string bankCode)
        {
            return Task.FromResult<BankProfile?>(null);
        }

        public Task<List<BankProfile>> GetAllBanksAsync()
        {
            return Task.FromResult(new List<BankProfile>());
        }

        public Task RefreshCacheAsync()
        {
            return Task.CompletedTask;
        }
    }
}
