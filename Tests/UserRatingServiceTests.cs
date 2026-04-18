using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BankProfiles.Tests;

public class UserRatingServiceTests
{
    private readonly IDbContextFactory<BankDbContext> _factory;
    private readonly UserRatingService _sut;

    public UserRatingServiceTests()
    {
        _factory = TestDbContextFactory.Create(Guid.NewGuid().ToString());

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RatingSettings:MinRating"] = "0",
                ["RatingSettings:MaxRating"] = "10"
            })
            .Build();

        _sut = new UserRatingService(_factory, TestDbContextFactory.CreateLogger<UserRatingService>(), configuration);
    }

    [Fact]
    public async Task SubmitRatingAsync_PersistsSubmissionAndCriterionRatings()
    {
        await EnsureBankExistsAsync();
        var criteria = await GetRequiredCriteriaAsync();

        var request = new UserRatingSubmissionRequest
        {
            BankCode = "bank-alpha",
            SubmitterIP = "127.0.0.1",
            Comment = "Recent service improvements were noticeable.",
            CriteriaRatings = criteria.ToDictionary(c => c.CriteriaId, c => 8.5m)
        };

        var result = await _sut.SubmitRatingAsync(request);

        Assert.True(result.Success);
        Assert.Equal(UserRatingSubmissionError.None, result.Error);
        Assert.Equal(9, result.RemainingSubmissions);

        await using var context = await _factory.CreateDbContextAsync();
        var submission = await context.UserRatingSubmissions.SingleAsync();
        Assert.Equal("127.0.0.1", submission.SubmitterIP);
        Assert.Equal("Recent service improvements were noticeable.", submission.Comment);
        Assert.Equal(8.5m, submission.ServiceRating);
        Assert.Equal(8.5m, submission.FeesRating);
        Assert.Equal(8.5m, submission.ConvenienceRating);
        Assert.Equal(8.5m, submission.DigitalServicesRating);
        Assert.Equal(8.5m, submission.CustomerSupportRating);

        var insertedRatings = await context.BankRatings
            .Where(r => r.UserRatingSubmissionId == submission.SubmissionId)
            .ToListAsync();

        Assert.Equal(5, insertedRatings.Count);
        Assert.All(insertedRatings, rating => Assert.Equal(8.5m, rating.RatingValue));
    }

    [Fact]
    public async Task SubmitRatingAsync_ReturnsMissingCriteria_WhenNotAllCriteriaProvided()
    {
        await EnsureBankExistsAsync();
        var criteria = await GetRequiredCriteriaAsync();

        var incompleteRatings = criteria.Take(4).ToDictionary(c => c.CriteriaId, c => 7.0m);
        var request = new UserRatingSubmissionRequest
        {
            BankCode = "bank-alpha",
            SubmitterIP = "127.0.0.1",
            CriteriaRatings = incompleteRatings
        };

        var result = await _sut.SubmitRatingAsync(request);

        Assert.False(result.Success);
        Assert.Equal(UserRatingSubmissionError.MissingCriteria, result.Error);
    }

    [Fact]
    public async Task SubmitRatingAsync_ReturnsInvalidRating_WhenValueOutsideRange()
    {
        await EnsureBankExistsAsync();
        var criteria = await GetRequiredCriteriaAsync();

        var ratings = criteria.ToDictionary(c => c.CriteriaId, c => c.Name == "Service" ? 11m : 7m);
        var request = new UserRatingSubmissionRequest
        {
            BankCode = "bank-alpha",
            SubmitterIP = "127.0.0.1",
            CriteriaRatings = ratings
        };

        var result = await _sut.SubmitRatingAsync(request);

        Assert.False(result.Success);
        Assert.Equal(UserRatingSubmissionError.InvalidRatingValue, result.Error);
    }

    [Fact]
    public async Task SubmitRatingAsync_EnforcesDailyRateLimit()
    {
        await EnsureBankExistsAsync();
        var criteria = await GetRequiredCriteriaAsync();
        const string ip = "127.0.0.1";

        for (var i = 0; i < 10; i++)
        {
            var request = new UserRatingSubmissionRequest
            {
                BankCode = "bank-alpha",
                SubmitterIP = ip,
                CriteriaRatings = criteria.ToDictionary(c => c.CriteriaId, c => 7.5m)
            };

            var result = await _sut.SubmitRatingAsync(request);
            Assert.True(result.Success);
        }

        var blocked = await _sut.SubmitRatingAsync(new UserRatingSubmissionRequest
        {
            BankCode = "bank-alpha",
            SubmitterIP = ip,
            CriteriaRatings = criteria.ToDictionary(c => c.CriteriaId, c => 7.5m)
        });

        Assert.False(blocked.Success);
        Assert.Equal(UserRatingSubmissionError.RateLimited, blocked.Error);
    }

    private async Task EnsureBankExistsAsync()
    {
        await using var context = await _factory.CreateDbContextAsync();
        await context.Database.EnsureCreatedAsync();

        var exists = await context.Banks.AnyAsync(b => b.BankCode == "bank-alpha");
        if (!exists)
        {
            context.Banks.Add(new Bank
            {
                BankCode = "bank-alpha",
                CreatedDate = DateTime.UtcNow,
                ViewCount = 0
            });
            await context.SaveChangesAsync();
        }
    }

    private async Task<List<UserRatingCriterionOption>> GetRequiredCriteriaAsync()
    {
        var criteria = await _sut.GetRequiredCriteriaAsync();
        Assert.Equal(5, criteria.Count);
        return criteria.ToList();
    }
}
