using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Models;
using BankProfiles.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BankProfiles.Tests;

public class BankOnboardingServiceTests
{
    private readonly IDbContextFactory<BankDbContext> _factory;
    private readonly StubBankDataService _bankDataService;
    private readonly BankOnboardingService _sut;

    public BankOnboardingServiceTests()
    {
        _factory = TestDbContextFactory.Create();
        _bankDataService = new StubBankDataService();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RatingSettings:MinRating"] = "0",
                ["RatingSettings:MaxRating"] = "10"
            })
            .Build();

        _sut = new BankOnboardingService(
            _factory,
            _bankDataService,
            new CountryService(),
            TestDbContextFactory.CreateLogger<BankOnboardingService>(),
            configuration);

        using var context = _factory.CreateDbContext();
        context.Database.EnsureCreated();

        if (!context.RatingCriterias.Any())
        {
            context.RatingCriterias.AddRange(
                new RatingCriteria { CriteriaId = 1, Name = "Service", DisplayOrder = 1 },
                new RatingCriteria { CriteriaId = 2, Name = "Fees", DisplayOrder = 2 },
                new RatingCriteria { CriteriaId = 3, Name = "Convenience", DisplayOrder = 3 },
                new RatingCriteria { CriteriaId = 4, Name = "Digital Services", DisplayOrder = 4 },
                new RatingCriteria { CriteriaId = 5, Name = "Customer Support", DisplayOrder = 5 });
            context.SaveChanges();
        }
    }

    [Fact]
    public async Task SubmitAsync_CreatesPendingSubmission()
    {
        var result = await _sut.SubmitAsync(new BankOnboardingSubmissionRequest
        {
            ProposedBankName = "North Star Bank",
            ProposedCountryCode = "us",
            ProposedWebsiteUrl = "https://northstar.example",
            SubmissionNotes = "Known regional bank with expanding digital products.",
            ContactEmail = "analyst@example.com",
            SubmitterIP = "127.0.0.1"
        });

        Assert.True(result.Success);
        Assert.NotNull(result.SubmissionId);

        await using var context = await _factory.CreateDbContextAsync();
        var submission = await context.BankOnboardingSubmissions.SingleAsync();

        Assert.Equal("North Star Bank", submission.ProposedBankName);
        Assert.Equal("us", submission.ProposedCountryCode);
        Assert.Equal(BankOnboardingStatuses.Pending, submission.Status);
    }

    [Fact]
    public async Task ApproveSubmissionAsync_PublishesEventsAndSeedsRatings()
    {
        var submitResult = await _sut.SubmitAsync(new BankOnboardingSubmissionRequest
        {
            ProposedBankName = "Orbit Finance",
            ProposedCountryCode = "uk",
            ProposedWebsiteUrl = "https://orbit.example",
            SubmitterIP = "127.0.0.2"
        });

        Assert.True(submitResult.Success);
        Assert.NotNull(submitResult.SubmissionId);

        var approvalResult = await _sut.ApproveSubmissionAsync(new BankOnboardingApprovalRequest
        {
            SubmissionId = submitResult.SubmissionId!.Value,
            BankCode = "bank-orbit-finance",
            BankName = "Orbit Finance",
            LegalName = "Orbit Finance PLC",
            Status = "active",
            CountryCode = "uk",
            Description = "Approved by admin test.",
            WebsiteUrl = "https://orbit.example",
            DefaultCriteriaRating = 8.0m
        });

        Assert.True(approvalResult.Success);
        Assert.False(string.IsNullOrWhiteSpace(approvalResult.BankCode));

        await using var context = await _factory.CreateDbContextAsync();

        var submission = await context.BankOnboardingSubmissions.SingleAsync();
        Assert.Equal(BankOnboardingStatuses.Approved, submission.Status);
        Assert.Equal(approvalResult.BankCode, submission.ApprovedBankCode);

        var bank = await context.Banks.FirstOrDefaultAsync(b => b.BankCode == approvalResult.BankCode);
        Assert.NotNull(bank);

        var ratingsCount = await context.BankRatings.CountAsync(r => r.BankId == bank!.BankId);
        Assert.Equal(5, ratingsCount);

        var eventCount = await context.MetricEvents.CountAsync(e => e.BankCode == approvalResult.BankCode);
        Assert.True(eventCount > 0);

        Assert.Equal(1, _bankDataService.RefreshCalls);
    }

    [Fact]
    public async Task RejectSubmissionAsync_MarksSubmissionRejected()
    {
        var submitResult = await _sut.SubmitAsync(new BankOnboardingSubmissionRequest
        {
            ProposedBankName = "Rejected Bank",
            ProposedCountryCode = "de",
            SubmitterIP = "127.0.0.3"
        });

        Assert.True(submitResult.Success);
        Assert.NotNull(submitResult.SubmissionId);

        var rejected = await _sut.RejectSubmissionAsync(
            submitResult.SubmissionId!.Value,
            "Insufficient supporting evidence.",
            "Need verified source links.");

        Assert.True(rejected);

        await using var context = await _factory.CreateDbContextAsync();
        var submission = await context.BankOnboardingSubmissions.SingleAsync();

        Assert.Equal(BankOnboardingStatuses.Rejected, submission.Status);
        Assert.Equal("Insufficient supporting evidence.", submission.RejectionReason);
    }

    private sealed class StubBankDataService : IBankDataService
    {
        public int RefreshCalls { get; private set; }

        public Task<BankProfile?> GetBankByCodeAsync(string bankCode)
            => Task.FromResult<BankProfile?>(null);

        public Task<List<BankProfile>> GetAllBanksAsync()
            => Task.FromResult(new List<BankProfile>());

        public Task RefreshCacheAsync()
        {
            RefreshCalls++;
            return Task.CompletedTask;
        }
    }
}
