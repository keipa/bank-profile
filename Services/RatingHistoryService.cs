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
        var intervalHours = _configuration.GetValue<int>("RatingSettings:HistorySnapshotIntervalHours");
        var interval = TimeSpan.FromHours(intervalHours);

        _logger.LogInformation("Rating History Service started. Interval: {Hours} hours", intervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var ratingService = scope.ServiceProvider.GetRequiredService<IRatingService>();
                
                await ratingService.AddRatingHistorySnapshotAsync();
                _logger.LogInformation("Rating history snapshot created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rating history snapshot");
            }
        }
    }
}
