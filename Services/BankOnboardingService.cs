using System.Text.RegularExpressions;
using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace BankProfiles.Web.Services;

public class BankOnboardingService : IBankOnboardingService
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "active",
        "inactive",
        "restricted",
        "under_review",
        "sanctioned",
        "closed"
    };

    private static readonly Regex SlugRegex = new("[^a-z0-9]+", RegexOptions.Compiled);

    private const int MaxSubmissionsPerDay = 5;
    private const int RateLimitWindowHours = 24;
    private const string FallbackClientIdentifier = "unknown-client";

    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly IBankDataService _bankDataService;
    private readonly ICountryService _countryService;
    private readonly ILogger<BankOnboardingService> _logger;
    private readonly decimal _minRating;
    private readonly decimal _maxRating;

    public BankOnboardingService(
        IDbContextFactory<BankDbContext> contextFactory,
        IBankDataService bankDataService,
        ICountryService countryService,
        ILogger<BankOnboardingService> logger,
        IConfiguration configuration)
    {
        _contextFactory = contextFactory;
        _bankDataService = bankDataService;
        _countryService = countryService;
        _logger = logger;
        _minRating = configuration.GetValue<decimal>("RatingSettings:MinRating", 0);
        _maxRating = configuration.GetValue<decimal>("RatingSettings:MaxRating", 10);
    }

    public async Task<BankOnboardingSubmissionResult> SubmitAsync(BankOnboardingSubmissionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var proposedBankName = request.ProposedBankName?.Trim() ?? string.Empty;
        var proposedCountryCode = request.ProposedCountryCode?.Trim().ToLowerInvariant() ?? string.Empty;
        var submitterIp = NormalizeClientIdentifier(request.SubmitterIP);

        if (proposedBankName.Length < 3 || proposedBankName.Length > 200)
        {
            return new BankOnboardingSubmissionResult
            {
                Success = false,
                ErrorMessage = "Bank name must be between 3 and 200 characters."
            };
        }

        if (_countryService.GetCountryInfo(proposedCountryCode) == null)
        {
            return new BankOnboardingSubmissionResult
            {
                Success = false,
                ErrorMessage = "Unsupported country code."
            };
        }

        if (!IsValidWebsite(request.ProposedWebsiteUrl))
        {
            return new BankOnboardingSubmissionResult
            {
                Success = false,
                ErrorMessage = "Website URL must be a valid HTTP or HTTPS URL."
            };
        }

        if (!await CheckRateLimitAsync(submitterIp))
        {
            return new BankOnboardingSubmissionResult
            {
                Success = false,
                ErrorMessage = "Daily submission limit reached. Please try again tomorrow.",
                RemainingSubmissions = 0
            };
        }

        await using var context = await _contextFactory.CreateDbContextAsync();

        var duplicatePending = await context.BankOnboardingSubmissions
            .AnyAsync(s => s.Status == BankOnboardingStatuses.Pending
                && s.ProposedCountryCode == proposedCountryCode
                && s.ProposedBankName.ToLower() == proposedBankName.ToLower());

        if (duplicatePending)
        {
            return new BankOnboardingSubmissionResult
            {
                Success = false,
                ErrorMessage = "A similar pending submission already exists for this bank."
            };
        }

        var submission = new BankOnboardingSubmission
        {
            ProposedBankName = proposedBankName,
            ProposedCountryCode = proposedCountryCode,
            ProposedWebsiteUrl = request.ProposedWebsiteUrl?.Trim(),
            SubmissionNotes = request.SubmissionNotes?.Trim(),
            ContactEmail = request.ContactEmail?.Trim(),
            SubmitterIP = submitterIp,
            SubmittedDate = DateTime.UtcNow,
            Status = BankOnboardingStatuses.Pending
        };

        context.BankOnboardingSubmissions.Add(submission);
        await context.SaveChangesAsync();

        var remainingSubmissions = await GetRemainingSubmissionsAsync(submitterIp);

        _logger.LogInformation(
            "New bank onboarding submission {SubmissionId} created for {BankName} ({CountryCode})",
            submission.SubmissionId,
            submission.ProposedBankName,
            submission.ProposedCountryCode);

        return new BankOnboardingSubmissionResult
        {
            Success = true,
            SubmissionId = submission.SubmissionId,
            RemainingSubmissions = remainingSubmissions
        };
    }

    public async Task<int> GetRemainingSubmissionsAsync(string? ipAddress)
    {
        var normalizedIp = NormalizeClientIdentifier(ipAddress);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var cutoff = DateTime.UtcNow.AddHours(-RateLimitWindowHours);

        var submissionCount = await context.BankOnboardingSubmissions
            .Where(s => s.SubmitterIP == normalizedIp && s.SubmittedDate >= cutoff)
            .CountAsync();

        return Math.Max(0, MaxSubmissionsPerDay - submissionCount);
    }

    public async Task<List<BankOnboardingSubmission>> GetPendingSubmissionsAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BankOnboardingSubmissions
            .Where(s => s.Status == BankOnboardingStatuses.Pending)
            .OrderBy(s => s.SubmittedDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<BankOnboardingSubmission?> GetSubmissionByIdAsync(int submissionId)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.BankOnboardingSubmissions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);
    }

    public async Task<BankOnboardingApprovalResult> ApproveSubmissionAsync(BankOnboardingApprovalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.SubmissionId <= 0)
        {
            return new BankOnboardingApprovalResult { Success = false, ErrorMessage = "Invalid submission identifier." };
        }

        if (string.IsNullOrWhiteSpace(request.BankName) || string.IsNullOrWhiteSpace(request.LegalName))
        {
            return new BankOnboardingApprovalResult { Success = false, ErrorMessage = "Bank name and legal name are required." };
        }

        if (!AllowedStatuses.Contains(request.Status))
        {
            return new BankOnboardingApprovalResult { Success = false, ErrorMessage = "Unsupported bank status." };
        }

        if (!IsValidWebsite(request.WebsiteUrl))
        {
            return new BankOnboardingApprovalResult { Success = false, ErrorMessage = "Website URL must be a valid HTTP or HTTPS URL." };
        }

        if (request.DefaultCriteriaRating < _minRating || request.DefaultCriteriaRating > _maxRating)
        {
            return new BankOnboardingApprovalResult
            {
                Success = false,
                ErrorMessage = $"Default criteria rating must be between {_minRating} and {_maxRating}."
            };
        }

        var countryCode = request.CountryCode.Trim().ToLowerInvariant();
        var countryInfo = _countryService.GetCountryInfo(countryCode);
        if (countryInfo == null)
        {
            return new BankOnboardingApprovalResult { Success = false, ErrorMessage = "Unsupported country code." };
        }

        var requestedBankCode = string.IsNullOrWhiteSpace(request.BankCode)
            ? GenerateBankCode(request.BankName)
            : NormalizeBankCode(request.BankCode);

        if (!ValidationHelper.IsValidBankCode(requestedBankCode))
        {
            return new BankOnboardingApprovalResult { Success = false, ErrorMessage = "Bank code format is invalid." };
        }

        await using var context = await _contextFactory.CreateDbContextAsync();

        var submission = await context.BankOnboardingSubmissions
            .FirstOrDefaultAsync(s => s.SubmissionId == request.SubmissionId);

        if (submission == null)
        {
            return new BankOnboardingApprovalResult { Success = false, ErrorMessage = "Submission not found." };
        }

        if (!string.Equals(submission.Status, BankOnboardingStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return new BankOnboardingApprovalResult { Success = false, ErrorMessage = "Only pending submissions can be approved." };
        }

        var finalBankCode = await EnsureUniqueBankCodeAsync(context, requestedBankCode);
        var bankProfile = BuildBankProfile(finalBankCode, countryInfo, request);

        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var events = EventMigrationService.FlattenProfileToEvents(
                bankProfile,
                $"Approved from onboarding submission #{submission.SubmissionId}");
            await AppendEventsAsync(context, events);

            var now = DateTime.UtcNow;
            var bank = await context.Banks.FirstOrDefaultAsync(b => b.BankCode == finalBankCode);
            if (bank == null)
            {
                bank = new Bank
                {
                    BankCode = finalBankCode,
                    ViewCount = 0,
                    CreatedDate = now
                };
                context.Banks.Add(bank);
            }

            var criteria = await context.RatingCriterias
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            if (criteria.Count == 0)
            {
                return new BankOnboardingApprovalResult
                {
                    Success = false,
                    ErrorMessage = "Rating criteria are missing. Cannot initialize ratings for new bank."
                };
            }

            var existingCriteriaIds = await context.BankRatings
                .Where(br => br.BankId == bank.BankId)
                .Select(br => br.CriteriaId)
                .ToHashSetAsync();

            foreach (var criterion in criteria.Where(c => !existingCriteriaIds.Contains(c.CriteriaId)))
            {
                context.BankRatings.Add(new BankRating
                {
                    BankId = bank.BankId,
                    CriteriaId = criterion.CriteriaId,
                    RatingValue = request.DefaultCriteriaRating,
                    RatingDate = now,
                    Notes = "Initial rating from onboarding approval"
                });
            }

            submission.Status = BankOnboardingStatuses.Approved;
            submission.ApprovedBankCode = finalBankCode;
            submission.ReviewNotes = request.ReviewNotes?.Trim();
            submission.RejectionReason = null;
            submission.ReviewedDate = now;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            try
            {
                await _bankDataService.RefreshCacheAsync();
            }
            catch (Exception cacheEx)
            {
                _logger.LogWarning(
                    cacheEx,
                    "Submission {SubmissionId} approved, but cache refresh failed",
                    submission.SubmissionId);
            }

            _logger.LogInformation(
                "Submission {SubmissionId} approved and published as bank {BankCode}",
                submission.SubmissionId,
                finalBankCode);

            return new BankOnboardingApprovalResult
            {
                Success = true,
                BankCode = finalBankCode
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to approve submission {SubmissionId}", submission.SubmissionId);
            return new BankOnboardingApprovalResult
            {
                Success = false,
                ErrorMessage = "Failed to publish approved submission. Please retry."
            };
        }
    }

    public async Task<bool> RejectSubmissionAsync(int submissionId, string rejectionReason, string? reviewNotes)
    {
        if (submissionId <= 0 || string.IsNullOrWhiteSpace(rejectionReason))
        {
            return false;
        }

        var normalizedReason = rejectionReason.Trim();
        var normalizedNotes = reviewNotes?.Trim();
        if (normalizedReason.Length > 1000 || (normalizedNotes?.Length ?? 0) > 2000)
        {
            return false;
        }

        await using var context = await _contextFactory.CreateDbContextAsync();
        var submission = await context.BankOnboardingSubmissions
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

        if (submission == null || !string.Equals(submission.Status, BankOnboardingStatuses.Pending, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        submission.Status = BankOnboardingStatuses.Rejected;
        submission.RejectionReason = normalizedReason;
        submission.ReviewNotes = normalizedNotes;
        submission.ReviewedDate = DateTime.UtcNow;

        await context.SaveChangesAsync();
        return true;
    }

    private async Task<bool> CheckRateLimitAsync(string? ipAddress)
    {
        var normalizedIp = NormalizeClientIdentifier(ipAddress);

        await using var context = await _contextFactory.CreateDbContextAsync();
        var cutoff = DateTime.UtcNow.AddHours(-RateLimitWindowHours);

        var submissionCount = await context.BankOnboardingSubmissions
            .Where(s => s.SubmitterIP == normalizedIp && s.SubmittedDate >= cutoff)
            .CountAsync();

        return submissionCount < MaxSubmissionsPerDay;
    }

    private async Task<string> EnsureUniqueBankCodeAsync(BankDbContext context, string requestedBankCode)
    {
        var baseCode = requestedBankCode;
        var candidate = requestedBankCode;
        var suffix = 2;

        while (await BankCodeExistsAsync(context, candidate))
        {
            var suffixPart = $"-{suffix++}";
            var maxBaseLength = Math.Max(1, 50 - suffixPart.Length);
            var trimmedBase = baseCode.Length <= maxBaseLength
                ? baseCode
                : baseCode[..maxBaseLength].TrimEnd('-');
            candidate = $"{trimmedBase}{suffixPart}";
        }

        return candidate;
    }

    private async Task<bool> BankCodeExistsAsync(BankDbContext context, string bankCode)
    {
        if (await context.Banks.AnyAsync(b => b.BankCode == bankCode))
        {
            return true;
        }

        if (await context.MetricEvents.AnyAsync(e => e.BankCode == bankCode))
        {
            return true;
        }

        if (await context.BankOnboardingSubmissions.AnyAsync(s => s.ApprovedBankCode == bankCode))
        {
            return true;
        }

        return await _bankDataService.GetBankByCodeAsync(bankCode) != null;
    }

    private static async Task AppendEventsAsync(BankDbContext context, List<MetricEvent> events)
    {
        if (events.Count == 0)
        {
            return;
        }

        var bankCode = events[0].BankCode;
        var nextSequence = (await context.MetricEvents
            .Where(e => e.BankCode == bankCode)
            .MaxAsync(e => (long?)e.EventSequence) ?? 0) + 1;

        var now = DateTime.UtcNow;
        foreach (var evt in events)
        {
            evt.EventSequence = nextSequence++;
            evt.CreatedDate = now;
            if (evt.EventVersion <= 0)
            {
                evt.EventVersion = 1;
            }
        }

        context.MetricEvents.AddRange(events);
    }

    private static BankProfile BuildBankProfile(string bankCode, CountryInfo country, BankOnboardingApprovalRequest request)
    {
        var now = DateTime.UtcNow;
        var defaultCurrency = string.IsNullOrWhiteSpace(country.Currency) ? "USD" : country.Currency;
        var defaultRatingOutOfFive = Math.Round((double)request.DefaultCriteriaRating / 2, 2);

        var availableCurrencies = new List<string> { defaultCurrency };
        if (!availableCurrencies.Contains("USD", StringComparer.OrdinalIgnoreCase))
        {
            availableCurrencies.Add("USD");
        }

        return new BankProfile
        {
            BankId = bankCode,
            Name = request.BankName.Trim(),
            LegalName = request.LegalName.Trim(),
            Status = request.Status.Trim().ToLowerInvariant(),
            CountryOfOwnerResidence = country.Name,
            HeadquartersCountry = country.Name,
            Jurisdiction = request.Jurisdiction?.Trim(),
            Overview = new BankOverview
            {
                Type = "commercial bank",
                Segment = "General Banking",
                Description = string.IsNullOrWhiteSpace(request.Description)
                    ? "Profile created from approved onboarding submission."
                    : request.Description.Trim()
            },
            Systems = new BankSystems
            {
                CardSystems = new List<string> { "visa", "mastercard" },
                SwiftAvailable = true,
                IbanSupported = !country.Code.Equals("us", StringComparison.OrdinalIgnoreCase),
                SepaAvailable = country.Code is "de" or "fr" or "es",
                LocalClearing = true,
                InstantTransfers = true,
                CryptoExposure = "none"
            },
            Currencies = new BankCurrencies
            {
                Available = availableCurrencies,
                BaseCurrency = defaultCurrency,
                MultiCurrencyAccounts = true,
                FxMarkupPercent = 1.0
            },
            Fees = new BankFees
            {
                Commissions = new FeesCommissions
                {
                    IncomingDomesticPercent = 0,
                    IncomingInternationalPercent = 0.5,
                    OutgoingDomesticPercent = 0.5,
                    OutgoingInternationalPercent = 1.0,
                    CashWithdrawalLocalAtmPercent = 0,
                    CashWithdrawalInternationalAtmPercent = 2.0,
                    FxMarkupPercent = 1.0
                },
                AccountFees = new FeesAccount
                {
                    AccountOpening = 0,
                    MonthlyMaintenance = 0,
                    AccountClosure = 0,
                    DormancyAfterMonths = 24,
                    DormancyFee = 0
                },
                CardFees = new FeesCard
                {
                    CardIssuance = 0,
                    PremiumCardAnnualFee = 0,
                    ReplacementCardFee = 10
                },
                TransferFees = new FeesTransfer
                {
                    SwiftPaymentProcessing = 15,
                    UrgentPaymentSurcharge = 10,
                    ChargebackHandling = 20
                }
            },
            Branches = new BankBranches
            {
                Count = 0,
                Countries = new List<string> { country.Name },
                AtmCount = 0,
                PartnerAtmNetwork = 0
            },
            Clients = new BankClients
            {
                Total = 0,
                Retail = 0,
                Business = 0,
                Corporate = 0,
                PrivateBanking = 0
            },
            Ratings = new BankRatings
            {
                Overall = defaultRatingOutOfFive,
                History = new List<RatingPoint>(),
                Source = "admin-onboarding",
                LastUpdated = now
            },
            Compliance = new BankCompliance
            {
                SanctionsRisk = "unknown",
                AmlStatus = "unknown",
                KycStatus = "unknown",
                GovernmentAffiliate = false,
                PepExposure = "unknown",
                OffshoreLinks = "unknown",
                FATCA = null,
                CRS = null,
                AuditPublished = null,
                DepositInsurance = null
            },
            DigitalChannels = new DigitalChannels
            {
                MobileApp = true,
                WebBanking = true,
                Ios = true,
                Android = true,
                ApiAccess = false,
                BiometricLogin = false,
                DeviceTrust = false,
                PushNotifications = true,
                UptimePercent = null,
                AverageAccountOpeningMinutes = null
            },
            Metadata = new BankMetadata
            {
                CreatedAt = now,
                UpdatedAt = now,
                LastReviewed = now,
                Source = "admin-onboarding",
                Confidence = 0.7
            },
            DefaultLanguage = country.Language,
            WebsiteUrl = request.WebsiteUrl?.Trim(),
            ContactInfo = string.IsNullOrWhiteSpace(request.ContactEmail)
                ? null
                : new ContactInfo { Email = request.ContactEmail.Trim() }
        };
    }

    private static string GenerateBankCode(string bankName)
    {
        var normalized = SlugRegex
            .Replace(bankName.Trim().ToLowerInvariant(), "-")
            .Trim('-');

        if (string.IsNullOrWhiteSpace(normalized))
        {
            normalized = "bank-new";
        }

        if (!normalized.StartsWith("bank-", StringComparison.Ordinal))
        {
            normalized = $"bank-{normalized}";
        }

        if (normalized.Length > 50)
        {
            normalized = normalized[..50].TrimEnd('-');
        }

        return normalized;
    }

    private static string NormalizeBankCode(string bankCode)
    {
        var normalized = SlugRegex
            .Replace(bankCode.Trim().ToLowerInvariant(), "-")
            .Trim('-');

        if (!normalized.StartsWith("bank-", StringComparison.Ordinal))
        {
            normalized = $"bank-{normalized}";
        }

        if (normalized.Length > 50)
        {
            normalized = normalized[..50].TrimEnd('-');
        }

        return normalized;
    }

    private static bool IsValidWebsite(string? websiteUrl)
    {
        if (string.IsNullOrWhiteSpace(websiteUrl))
        {
            return true;
        }

        if (!Uri.TryCreate(websiteUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }

    private static string NormalizeClientIdentifier(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return FallbackClientIdentifier;
        }

        return ipAddress.Trim();
    }
}
