using BankProfiles.Web.Components;
using BankProfiles.Web.Data;
using BankProfiles.Web.Services;
using BankProfiles.Web.HostedServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
builder.Services.AddScoped<IBankDataService, BankDataService>();
builder.Services.AddScoped<IRatingService, RatingService>();
builder.Services.AddScoped<IViewCountService, ViewCountService>();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IChartDataService, ChartDataService>();
builder.Services.AddSingleton<ICountryService, CountryService>();

// Register background services
builder.Services.AddHostedService<RatingHistoryService>();

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

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
