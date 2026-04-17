using BankProfiles.Web.Models;
using BankProfiles.Web.Data.Entities;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankProfiles.Web.Services;

public interface IEventMigrationService
{
    Task<MigrationResult> MigrateFromJsonAsync(bool dryRun = false);
    Task<MigrationResult> MigrateSingleBankAsync(string bankCode, bool dryRun = false);
}

public class MigrationResult
{
    public int BanksProcessed { get; set; }
    public int BanksSkipped { get; set; }
    public int EventsCreated { get; set; }
    public List<string> Errors { get; set; } = new();
    public bool DryRun { get; set; }
}

public class EventMigrationService : IEventMigrationService
{
    private readonly IEventStoreService _eventStoreService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EventMigrationService> _logger;
    private readonly string _dataDirectory;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EventMigrationService(
        IEventStoreService eventStoreService,
        IConfiguration configuration,
        ILogger<EventMigrationService> logger)
    {
        _eventStoreService = eventStoreService;
        _configuration = configuration;
        _logger = logger;
        _dataDirectory = _configuration.GetValue<string>("BankDataSettings:DataDirectory")
            ?? "wwwroot/data/banks";
    }

    public async Task<MigrationResult> MigrateFromJsonAsync(bool dryRun = false)
    {
        var result = new MigrationResult { DryRun = dryRun };

        if (!Directory.Exists(_dataDirectory))
        {
            _logger.LogWarning("Data directory not found: {Directory}", _dataDirectory);
            result.Errors.Add($"Data directory not found: {_dataDirectory}");
            return result;
        }

        var jsonFiles = Directory.GetFiles(_dataDirectory, "*.json");
        _logger.LogInformation("Found {Count} JSON files to migrate", jsonFiles.Length);

        foreach (var filePath in jsonFiles)
        {
            var bankCode = Path.GetFileNameWithoutExtension(filePath);
            try
            {
                var singleResult = await MigrateSingleBankAsync(bankCode, dryRun);
                result.BanksProcessed += singleResult.BanksProcessed;
                result.BanksSkipped += singleResult.BanksSkipped;
                result.EventsCreated += singleResult.EventsCreated;
                result.Errors.AddRange(singleResult.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate bank {BankCode}", bankCode);
                result.Errors.Add($"Failed to migrate {bankCode}: {ex.Message}");
            }
        }

        _logger.LogInformation(
            "Migration complete. Banks: {Processed} processed, {Skipped} skipped. Events: {Events}. Errors: {Errors}. DryRun: {DryRun}",
            result.BanksProcessed, result.BanksSkipped, result.EventsCreated, result.Errors.Count, dryRun);

        return result;
    }

    public async Task<MigrationResult> MigrateSingleBankAsync(string bankCode, bool dryRun = false)
    {
        var result = new MigrationResult { DryRun = dryRun };

        // Idempotent: skip if events already exist
        if (await _eventStoreService.HasEventsAsync(bankCode))
        {
            _logger.LogInformation("Bank {BankCode} already has events, skipping", bankCode);
            result.BanksSkipped = 1;
            return result;
        }

        var filePath = Path.Combine(_dataDirectory, $"{bankCode}.json");
        if (!File.Exists(filePath))
        {
            result.Errors.Add($"JSON file not found: {filePath}");
            return result;
        }

        var json = await File.ReadAllTextAsync(filePath);
        var profile = JsonSerializer.Deserialize<BankProfile>(json, JsonOptions);
        if (profile == null)
        {
            result.Errors.Add($"Failed to deserialize {bankCode}");
            return result;
        }

        var events = FlattenProfileToEvents(profile);
        result.EventsCreated = events.Count;
        result.BanksProcessed = 1;

        if (!dryRun && events.Count > 0)
        {
            await _eventStoreService.AppendEventsAsync(events);
            _logger.LogInformation("Migrated bank {BankCode}: {Count} events", bankCode, events.Count);
        }
        else
        {
            _logger.LogInformation("Dry run for bank {BankCode}: would create {Count} events", bankCode, events.Count);
        }

        return result;
    }

    private static List<MetricEvent> FlattenProfileToEvents(BankProfile profile)
    {
        var events = new List<MetricEvent>();
        var country = profile.HeadquartersCountry ?? "unknown";
        var now = DateTime.UtcNow;

        FlattenObject(profile, Array.Empty<string>(), events, profile.BankId, country, now);
        return events;
    }

    private static void FlattenObject(
        object obj,
        string[] parentPath,
        List<MetricEvent> events,
        string bankCode,
        string country,
        DateTime createdDate)
    {
        foreach (var prop in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                continue;

            var jsonName = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;
            var currentPath = parentPath.Append(jsonName).ToArray();
            var value = prop.GetValue(obj);

            if (value == null)
                continue;

            var propType = prop.PropertyType;
            var underlying = Nullable.GetUnderlyingType(propType) ?? propType;

            if (IsLeafType(underlying))
            {
                var metricName = string.Join(".", currentPath);
                var metricType = ResolveMetricType(underlying, metricName);
                var serializedValue = JsonSerializer.Serialize(value, JsonOptions);

                events.Add(new MetricEvent
                {
                    BankCode = bankCode,
                    Country = country,
                    MetricName = metricName,
                    MetricValue = serializedValue,
                    MetricType = metricType,
                    Comment = "Initial migration from JSON",
                    CreatedDate = createdDate,
                    EventVersion = 1
                });
            }
            else if (IsNavigableType(underlying))
            {
                FlattenObject(value, currentPath, events, bankCode, country, createdDate);
            }
            else if (IsListOfComplexType(propType))
            {
                // Serialize entire list as a single event (e.g., redFlags, growthHistory)
                var metricName = string.Join(".", currentPath);
                var serializedValue = JsonSerializer.Serialize(value, JsonOptions);

                events.Add(new MetricEvent
                {
                    BankCode = bankCode,
                    Country = country,
                    MetricName = metricName,
                    MetricValue = serializedValue,
                    MetricType = "List",
                    Comment = "Initial migration from JSON",
                    CreatedDate = createdDate,
                    EventVersion = 1
                });
            }
        }
    }

    private static string ResolveMetricType(Type type, string metricName)
    {
        if (type == typeof(bool)) return "Boolean";
        if (type == typeof(int) || type == typeof(long)) return "Numeric";
        if (type == typeof(double) || type == typeof(decimal))
        {
            if (metricName.Contains("Percent", StringComparison.OrdinalIgnoreCase))
                return "Percentage";
            if (metricName.Contains("Fee", StringComparison.OrdinalIgnoreCase)
                || metricName.Contains("Maintenance", StringComparison.OrdinalIgnoreCase)
                || metricName.Contains("Surcharge", StringComparison.OrdinalIgnoreCase))
                return "Currency";
            return "Numeric";
        }
        if (type == typeof(DateTime)) return "Text";
        if (type == typeof(string)) return "Text";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) return "List";
        return "Text";
    }

    private static bool IsLeafType(Type type)
    {
        return type.IsPrimitive
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(double)
            || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)
                && type.GetGenericArguments()[0] == typeof(string));
    }

    private static bool IsNavigableType(Type type)
    {
        return type.IsClass
            && type != typeof(string)
            && !type.IsGenericType
            && type.Namespace?.StartsWith("BankProfiles.Web.Models") == true;
    }

    private static bool IsListOfComplexType(Type type)
    {
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(List<>))
            return false;
        var elementType = type.GetGenericArguments()[0];
        return elementType != typeof(string) && elementType.IsClass;
    }
}
