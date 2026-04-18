using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankProfiles.Web.Services;

public class EventProjectionService : IEventProjectionService
{
    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly ILogger<EventProjectionService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Cache property metadata to avoid repeated reflection
    private static readonly Dictionary<string, PropertyPathInfo> PropertyPathCache = BuildPropertyPathCache();

    public EventProjectionService(
        IDbContextFactory<BankDbContext> contextFactory,
        ILogger<EventProjectionService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<BankProfile?> ProjectBankProfileAsync(string bankCode)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var events = await context.MetricEvents
            .Where(e => e.BankCode == bankCode)
            .OrderBy(e => e.EventSequence)
            .AsNoTracking()
            .ToListAsync();

        if (events.Count == 0)
            return null;

        return ProjectFromEvents(events);
    }

    public BankProfile? ProjectFromEvents(List<MetricEvent> events)
    {
        if (events.Count == 0)
            return null;

        var profile = CreateEmptyProfile();
        var latestValues = new Dictionary<string, MetricEvent>();

        // Build a map of latest values per metric (last event wins)
        foreach (var evt in events.OrderBy(e => e.EventSequence))
        {
            latestValues[evt.MetricName] = evt;
        }

        foreach (var kvp in latestValues)
        {
            try
            {
                ApplyMetric(profile, kvp.Key, kvp.Value);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to apply metric {MetricName} for bank {BankCode}",
                    kvp.Key, kvp.Value.BankCode);
            }
        }

        return profile;
    }

    public static bool TryGetMetricPropertyType(string metricName, out Type? propertyType)
    {
        if (string.IsNullOrWhiteSpace(metricName))
        {
            propertyType = null;
            return false;
        }

        if (PropertyPathCache.TryGetValue(metricName.Trim(), out var pathInfo))
        {
            propertyType = pathInfo.PropertyType;
            return true;
        }

        propertyType = null;
        return false;
    }

    private static void ApplyMetric(BankProfile profile, string metricName, MetricEvent evt)
    {
        if (!PropertyPathCache.TryGetValue(metricName, out var pathInfo))
            return;

        var target = NavigateToParent(profile, pathInfo.PathSegments);
        if (target == null)
            return;

        var value = DeserializeValue(evt.MetricValue, pathInfo.PropertyType, evt.MetricType);
        pathInfo.Setter(target, value);
    }

    private static object? NavigateToParent(object root, string[] pathSegments)
    {
        var current = root;
        // Navigate all segments except the last (which is the property to set)
        for (int i = 0; i < pathSegments.Length - 1; i++)
        {
            var prop = current.GetType().GetProperty(pathSegments[i],
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null) return null;

            var next = prop.GetValue(current);
            if (next == null)
            {
                // Auto-initialize nested objects
                next = Activator.CreateInstance(prop.PropertyType);
                if (next == null) return null;
                prop.SetValue(current, next);
            }
            current = next;
        }
        return current;
    }

    private static object? DeserializeValue(string jsonValue, Type targetType, string metricType)
    {
        try
        {
            return JsonSerializer.Deserialize(jsonValue, targetType, JsonOptions);
        }
        catch
        {
            // Fallback: try direct conversion for primitives
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying == typeof(bool) && bool.TryParse(jsonValue, out var b)) return b;
            if (underlying == typeof(int) && int.TryParse(jsonValue, out var i)) return i;
            if (underlying == typeof(long) && long.TryParse(jsonValue, out var l)) return l;
            if (underlying == typeof(double) && double.TryParse(jsonValue, out var d)) return d;
            if (underlying == typeof(decimal) && decimal.TryParse(jsonValue, out var dec)) return dec;
            if (underlying == typeof(DateTime) && DateTime.TryParse(jsonValue, out var dt)) return dt;
            if (underlying == typeof(string)) return jsonValue;

            return null;
        }
    }

    private static BankProfile CreateEmptyProfile()
    {
        return new BankProfile
        {
            BankId = string.Empty,
            Name = string.Empty,
            LegalName = string.Empty,
            Status = "active",
            CountryOfOwnerResidence = string.Empty,
            HeadquartersCountry = string.Empty,
            Systems = new BankSystems
            {
                CardSystems = new List<string>(),
                SwiftAvailable = false,
                IbanSupported = false,
                SepaAvailable = false
            },
            Currencies = new BankCurrencies
            {
                Available = new List<string>()
            },
            Fees = new BankFees
            {
                Commissions = new FeesCommissions(),
                AccountFees = new FeesAccount(),
                CardFees = new FeesCard(),
                TransferFees = new FeesTransfer()
            },
            Branches = new BankBranches { Count = 0 },
            Clients = new BankClients { Total = 0 },
            Ratings = new BankRatings { Overall = 0 },
            Compliance = new BankCompliance
            {
                SanctionsRisk = "unknown",
                AmlStatus = "unknown",
                KycStatus = "unknown"
            },
            DigitalChannels = new DigitalChannels()
        };
    }

    /// <summary>
    /// Builds a static cache of all navigable property paths for BankProfile.
    /// Uses JsonPropertyName attributes for dot-notation path mapping.
    /// </summary>
    private static Dictionary<string, PropertyPathInfo> BuildPropertyPathCache()
    {
        var cache = new Dictionary<string, PropertyPathInfo>(StringComparer.OrdinalIgnoreCase);
        BuildPaths(typeof(BankProfile), Array.Empty<string>(), Array.Empty<string>(), cache);
        return cache;
    }

    private static void BuildPaths(
        Type type,
        string[] jsonPath,
        string[] clrPath,
        Dictionary<string, PropertyPathInfo> cache)
    {
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null)
                continue;

            var jsonName = prop.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? prop.Name;
            var currentJsonPath = jsonPath.Append(jsonName).ToArray();
            var currentClrPath = clrPath.Append(prop.Name).ToArray();

            if (IsLeafType(prop.PropertyType))
            {
                var pathKey = string.Join(".", currentJsonPath);
                cache[pathKey] = new PropertyPathInfo
                {
                    PathSegments = currentClrPath,
                    PropertyType = prop.PropertyType,
                    Setter = (obj, value) => prop.SetValue(obj, value)
                };
            }
            else if (IsNavigableType(prop.PropertyType))
            {
                BuildPaths(prop.PropertyType, currentJsonPath, currentClrPath, cache);
            }
        }
    }

    private static bool IsLeafType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive
            || underlying == typeof(string)
            || underlying == typeof(decimal)
            || underlying == typeof(DateTime)
            || underlying == typeof(double)
            || (underlying.IsGenericType && underlying.GetGenericTypeDefinition() == typeof(List<>));
    }

    private static bool IsNavigableType(Type type)
    {
        return type.IsClass
            && type != typeof(string)
            && !type.IsGenericType
            && type.Namespace?.StartsWith("BankProfiles.Web.Models") == true;
    }

    private class PropertyPathInfo
    {
        public required string[] PathSegments { get; init; }
        public required Type PropertyType { get; init; }
        public required Action<object, object?> Setter { get; init; }
    }
}
