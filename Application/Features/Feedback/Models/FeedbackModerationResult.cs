namespace BankProfiles.Web.Application.Features.Feedback.Models;

public sealed class FeedbackModerationResult
{
   public bool Success { get; init; }
   public FeedbackModerationError Error { get; init; } = FeedbackModerationError.None;
   public string? ErrorMessage { get; init; }
   public long? AppliedEventId { get; init; }
}