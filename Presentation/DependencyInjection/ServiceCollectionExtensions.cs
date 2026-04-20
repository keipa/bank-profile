using BankProfiles.Web.Application.Features.BankProfiles.Services;
using BankProfiles.Web.Application.Features.Caching.Services;
using BankProfiles.Web.Application.Features.EventSourcing.Services;
using BankProfiles.Web.Application.Features.Feedback.Services;
using BankProfiles.Web.Application.Features.Localization.Services;
using BankProfiles.Web.Application.Features.Onboarding.Services;
using BankProfiles.Web.Application.Features.Ratings.Services;
using BankProfiles.Web.Application.Interfaces.Repositories.EventSourcing;
using BankProfiles.Web.Application.Interfaces.Services.BankProfiles;
using BankProfiles.Web.Application.Interfaces.Services.Common;
using BankProfiles.Web.Application.Interfaces.Services.EventSourcing;
using BankProfiles.Web.Application.Interfaces.Services.Feedback;
using BankProfiles.Web.Application.Interfaces.Services.Localization;
using BankProfiles.Web.Application.Interfaces.Services.Onboarding;
using BankProfiles.Web.Application.Interfaces.Services.Ratings;
using BankProfiles.Web.Infrastructure.Persistence.Repositories.EventSourcing;
using BankProfiles.Web.Presentation.Services.Modal;

namespace BankProfiles.Web.Presentation.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBankProfileServices(this IServiceCollection services)
    {
        services.AddScoped<ICacheManager, CacheManager>();
        services.AddScoped<IEventStoreService, EventStoreService>();
        services.AddScoped<IEventProjectionService, EventProjectionService>();
        services.AddScoped<IEventMigrationService, EventMigrationService>();
        services.AddScoped<IBankDataService, BankDataService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<IViewCountService, ViewCountService>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IChartDataService, ChartDataService>();
        services.AddScoped<IBankMetricsExtractorService, BankMetricsExtractorService>();
        services.AddScoped<INumberFormatterService, NumberFormatterService>();
        services.AddScoped<IFeedbackService, FeedbackService>();
        services.AddScoped<IBankOnboardingService, BankOnboardingService>();
        services.AddScoped<IUserRatingService, UserRatingService>();
        services.AddScoped<ModalService>();
        services.AddSingleton<ICountryService, CountryService>();
        services.AddSingleton<ICountryCodeMapperService, CountryCodeMapperService>();

        services.AddHostedService<RatingHistoryService>();

        return services;
    }
}
