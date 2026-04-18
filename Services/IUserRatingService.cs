namespace BankProfiles.Web.Services;

public interface IUserRatingService
{
    Task<IReadOnlyList<UserRatingCriterionOption>> GetRequiredCriteriaAsync();
    Task<UserRatingSubmissionResult> SubmitRatingAsync(UserRatingSubmissionRequest request);
    Task<bool> CheckRateLimitAsync(string? ipAddress);
    Task<int> GetRemainingSubmissionsAsync(string? ipAddress);
}
