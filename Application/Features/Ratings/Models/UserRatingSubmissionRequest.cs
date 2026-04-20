namespace BankProfiles.Web.Application.Features.Ratings.Models;

public sealed class UserRatingSubmissionRequest
{
   public string BankCode { get; init; } = string.Empty;
   public IReadOnlyDictionary<int, decimal> CriteriaRatings { get; init; } = new Dictionary<int, decimal>();
   public string? Comment { get; init; }
   public string? SubmitterIP { get; init; }
}