using BankProfiles.Web.Application.Features.Ratings.Models;

namespace BankProfiles.Web.Application.Interfaces.Services.Ratings;

public interface IUserRatingService
{
    Task<IReadOnlyList<UserRatingCriterionOption>> GetRequiredCriteriaAsync();
    Task<UserRatingSubmissionResult> SubmitRatingAsync(UserRatingSubmissionRequest request);
    Task<bool> CheckRateLimitAsync(string? ipAddress);
    Task<int> GetRemainingSubmissionsAsync(string? ipAddress);
}
