using BankProfiles.Web.Components;
using BankProfiles.Web.Presentation.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using System.Globalization;
using BankProfiles.Web.Infrastructure.Persistence.DbContext;
using BankProfiles.Web.Infrastructure.Persistence.Seeders;
using BankProfiles.Web.Presentation.Middleware;

static TimeSpan ParseInvariantDuration(string? rawValue, string settingKey, string fallbackValue)
{
    var value = string.IsNullOrWhiteSpace(rawValue) ? fallbackValue : rawValue;
    if (TimeSpan.TryParseExact(value, "c", CultureInfo.InvariantCulture, out var parsed))
    {
        return parsed;
    }

    throw new InvalidOperationException(
        $"Invalid duration value for SignalR setting '{settingKey}': '{value}'. Expected invariant constant format (for example, 00:03:00).");
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure SignalR with circuit limits
builder.Services.AddServerSideBlazor(options =>
{
    var signalRSettings = builder.Configuration.GetSection("SignalRSettings");
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DisconnectedCircuitMaxRetained = signalRSettings.GetValue<int>("DisconnectedCircuitMaxRetained");
    options.DisconnectedCircuitRetentionPeriod = ParseInvariantDuration(
        signalRSettings.GetValue<string>("DisconnectedCircuitRetentionPeriod"),
        "DisconnectedCircuitRetentionPeriod",
        "00:03:00");
    options.JSInteropDefaultCallTimeout = ParseInvariantDuration(
        signalRSettings.GetValue<string>("JSInteropDefaultCallTimeout"),
        "JSInteropDefaultCallTimeout",
        "00:01:00");
}).AddHubOptions(options =>
{
    var signalRSettings = builder.Configuration.GetSection("SignalRSettings");
    options.MaximumReceiveMessageSize = signalRSettings.GetValue<int>("MaximumReceiveMessageSize");
});

// Add HttpContextAccessor for cookie access
builder.Services.AddHttpContextAccessor();

// Add DbContext Factory for thread-safe concurrent operations
builder.Services.AddPooledDbContextFactory<BankDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BankDatabase")));

// Configure Memory Cache with size limit
builder.Services.AddMemoryCache(options =>
{
    var cacheSizeMB = builder.Configuration.GetValue<int>("CacheSettings:SizeLimitMB");
    options.SizeLimit = cacheSizeMB * 1024 * 1024; // Convert MB to bytes
});

// Configure Cookie Policy for GDPR compliance
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Lax;
});

// Register application services
builder.Services.AddBankProfileServices();

// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    var rateLimitSettings = builder.Configuration.GetSection("RateLimitSettings");
    var permitLimit = rateLimitSettings.GetValue<int>("PermitLimit");
    var windowSeconds = rateLimitSettings.GetValue<int>("WindowSeconds");
    var queueLimit = rateLimitSettings.GetValue<int>("QueueLimit");

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = queueLimit,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.HttpContext.Request.Path;
        logger.LogWarning("Rate limit exceeded for IP: {IP}, Path: {Path}", ip, path);
        await context.HttpContext.Response.WriteAsync("Rate limit exceeded. Please try again later.", token);
    };
});

var app = builder.Build();

var runSeedHistoricalData = args.Any(arg =>
    string.Equals(arg, "--seed-historical-data", StringComparison.OrdinalIgnoreCase));
var runReseedHistoricalData = args.Any(arg =>
    string.Equals(arg, "--reseed-historical-data", StringComparison.OrdinalIgnoreCase));

if (runSeedHistoricalData || runReseedHistoricalData)
{
    await using var scope = app.Services.CreateAsyncScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var historicalDataSeeder = scope.ServiceProvider.GetRequiredService<HistoricalDataSeeder>();
    var metricEventHistoricalSeeder = scope.ServiceProvider.GetRequiredService<MetricEventHistoricalSeeder>();
    var forceReseed = runReseedHistoricalData;

    await metricEventHistoricalSeeder.SeedMetricEventHistoryAsync(forceReseed, CancellationToken.None);
    await historicalDataSeeder.SeedHistoricalDataAsync(forceReseed, CancellationToken.None);

    logger.LogInformation("Historical seeding command completed. Force reseed: {ForceReseed}", forceReseed);
    return;
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseCookiePolicy();

app.UseRateLimiter();

app.UseMiddleware<CircuitRateLimitingMiddleware>();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
