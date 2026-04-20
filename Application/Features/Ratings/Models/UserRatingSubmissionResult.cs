namespace BankProfiles.Web.Application.Features.Ratings.Models;

public sealed class UserRatingSubmissionResult
{
   public bool Success { get; init; }
   public UserRatingSubmissionError Error { get; init; } = UserRatingSubmissionError.None;
   public int RemainingSubmissions { get; init; }
}