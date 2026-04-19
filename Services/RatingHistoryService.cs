using BankProfiles.Web.Services;

namespace BankProfiles.Web.HostedServices;

public class RatingHistoryService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RatingHistoryService> _logger;
    private readonly IConfiguration _configuration;

    public RatingHistoryService(
        IServiceProvider serviceProvider,
        ILogger<RatingHistoryService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configuredIntervalHours = _configuration.GetValue<int>("RatingSettings:HistorySnapshotIntervalHours");
        var intervalHours = configuredIntervalHours < 1 ? 24 : configuredIntervalHours;
        if (configuredIntervalHours < 1)
        {
            _logger.LogWarning(
                "Invalid RatingSettings:HistorySnapshotIntervalHours value {ConfiguredHours}. Falling back to {FallbackHours} hours.",
                configuredIntervalHours,
                intervalHours);
        }

        var interval = TimeSpan.FromHours(intervalHours);

        _logger.LogInformation("Rating History Service started. Interval: {Hours} hours", intervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var ratingService = scope.ServiceProvider.GetRequiredService<IRatingService>();
                
                await ratingService.AddRatingHistorySnapshotAsync(stoppingToken);
                _logger.LogInformation("Rating history snapshot created successfully");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rating history snapshot");
            }
        }

        _logger.LogInformation("Rating History Service stopped");
    }
}
