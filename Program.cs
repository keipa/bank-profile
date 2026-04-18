using BankProfiles.Web.Components;
using BankProfiles.Web.Data;
using BankProfiles.Web.Services;
using BankProfiles.Web.HostedServices;
using BankProfiles.Web.Middleware;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

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
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.Parse(signalRSettings.GetValue<string>("DisconnectedCircuitRetentionPeriod") ?? "00:03:00");
    options.JSInteropDefaultCallTimeout = TimeSpan.Parse(signalRSettings.GetValue<string>("JSInteropDefaultCallTimeout") ?? "00:01:00");
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
builder.Services.AddScoped<ICacheManager, CacheManager>();
builder.Services.AddScoped<IEventStoreService, EventStoreService>();
builder.Services.AddScoped<IEventProjectionService, EventProjectionService>();
builder.Services.AddScoped<IEventMigrationService, EventMigrationService>();
builder.Services.AddScoped<IBankDataService, BankDataService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IViewCountService, ViewCountService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IChartDataService, ChartDataService>();
builder.Services.AddScoped<IBankMetricsExtractorService, BankMetricsExtractorService>();
builder.Services.AddScoped<INumberFormatterService, NumberFormatterService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IBankOnboardingService, BankOnboardingService>();
builder.Services.AddScoped<IUserRatingService, UserRatingService>();
builder.Services.AddScoped<ModalService>();
builder.Services.AddSingleton<ICountryService, CountryService>();
builder.Services.AddSingleton<ICountryCodeMapperService, CountryCodeMapperService>();

// Register background services
builder.Services.AddHostedService<RatingHistoryService>();

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
