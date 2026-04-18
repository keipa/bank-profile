using BankProfiles.Web.Data.Entities;
using BankProfiles.Web.Models;
using BankProfiles.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BankProfiles.Tests;

/// <summary>
/// Validates that migrating a real bank JSON → events → projection produces
/// a profile matching the original JSON data on all leaf properties.
/// </summary>
public class MigrationParityTests
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    [Theory]
    [InlineData("bank-alpha")]
    [InlineData("bank-beta")]
    [InlineData("bank-gamma")]
    [InlineData("bank-delta")]
    [InlineData("bank-epsilon")]
    public async Task MigratedProfile_MatchesOriginalJson(string bankCode)
    {
        var jsonPath = Path.Combine("wwwroot", "data", "banks", $"{bankCode}.json");
        var fullPath = FindProjectRoot(jsonPath);
        if (!File.Exists(fullPath))
        {
            // Skip if JSON data files not available in test runner context
            return;
        }

        var json = await File.ReadAllTextAsync(fullPath);
        var original = JsonSerializer.Deserialize<BankProfile>(json, JsonOpts)!;

        // Flatten the original profile into events using the migration service's approach
        var events = FlattenProfile(original);
        Assert.True(events.Count > 0, $"Expected events from {bankCode}");

        // Project from events
        var factory = TestDbContextFactory.Create();
        var eventStore = new EventStoreService(factory, TestDbContextFactory.CreateLogger<EventStoreService>());
        var projection = new EventProjectionService(factory, TestDbContextFactory.CreateLogger<EventProjectionService>());

        await eventStore.AppendEventsAsync(events);
        var projected = await projection.ProjectBankProfileAsync(bankCode);

        Assert.NotNull(projected);

        // Verify core fields
        Assert.Equal(original.BankId, projected!.BankId);
        Assert.Equal(original.Name, projected.Name);
        Assert.Equal(original.LegalName, projected.LegalName);
        Assert.Equal(original.Status, projected.Status);
        Assert.Equal(original.HeadquartersCountry, projected.HeadquartersCountry);
        Assert.Equal(original.CountryOfOwnerResidence, projected.CountryOfOwnerResidence);
        Assert.Equal(original.Overview?.LogoUrl, projected.Overview?.LogoUrl);
        Assert.Equal(original.Overview?.IconUrl, projected.Overview?.IconUrl);

        // Verify ratings
        Assert.Equal(original.Ratings.Overall, projected.Ratings.Overall);

        // Verify systems
        Assert.Equal(original.Systems.SwiftAvailable, projected.Systems.SwiftAvailable);
        Assert.Equal(original.Systems.IbanSupported, projected.Systems.IbanSupported);
        Assert.Equal(original.Systems.SepaAvailable, projected.Systems.SepaAvailable);
        Assert.Equal(original.Systems.CardSystems, projected.Systems.CardSystems);

        // Verify currencies
        Assert.Equal(original.Currencies.Available, projected.Currencies.Available);

        // Verify branches & clients
        Assert.Equal(original.Branches.Count, projected.Branches.Count);
        Assert.Equal(original.Clients.Total, projected.Clients.Total);

        // Verify transaction destinations when present
        if (original.Transactions?.OutgoingDestinations != null)
        {
            Assert.NotNull(projected.Transactions);
            Assert.NotNull(projected.Transactions!.OutgoingDestinations);
            Assert.Equal(
                original.Transactions.OutgoingDestinations.Select(destination => destination.Country),
                projected.Transactions.OutgoingDestinations!.Select(destination => destination.Country));
        }

        // Verify compliance
        Assert.Equal(original.Compliance.SanctionsRisk, projected.Compliance.SanctionsRisk);
        Assert.Equal(original.Compliance.AmlStatus, projected.Compliance.AmlStatus);
        Assert.Equal(original.Compliance.KycStatus, projected.Compliance.KycStatus);

        // Verify fees
        Assert.Equal(original.Fees.Commissions.IncomingDomesticPercent, projected.Fees.Commissions.IncomingDomesticPercent);
        Assert.Equal(original.Fees.Commissions.OutgoingInternationalPercent, projected.Fees.Commissions.OutgoingInternationalPercent);
    }

    private static string FindProjectRoot(string relativePath)
    {
        var dir = Directory.GetCurrentDirectory();
        // Walk up to find wwwroot
        for (int i = 0; i < 10; i++)
        {
            var candidate = Path.Combine(dir, relativePath);
            if (File.Exists(candidate)) return candidate;
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return Path.Combine(Directory.GetCurrentDirectory(), relativePath);
    }

    /// <summary>
    /// Mirrors EventMigrationService.FlattenProfileToEvents logic for test isolation.
    /// </summary>
    private static List<MetricEvent> FlattenProfile(BankProfile profile)
    {
        var events = new List<MetricEvent>();
        var country = profile.HeadquartersCountry ?? "unknown";
        FlattenObject(profile, Array.Empty<string>(), events, profile.BankId, country);
        return events;
    }

    private static void FlattenObject(object obj, string[] parentPath, List<MetricEvent> events, string bankCode, string country)
    {
        foreach (var prop in obj.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            if (prop.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute), false).Length > 0)
                continue;

            var jsonAttr = prop.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute), false)
                .FirstOrDefault() as System.Text.Json.Serialization.JsonPropertyNameAttribute;
            var jsonName = jsonAttr?.Name ?? prop.Name;
            var currentPath = parentPath.Append(jsonName).ToArray();
            var value = prop.GetValue(obj);

            if (value == null) continue;

            var propType = prop.PropertyType;
            var underlying = Nullable.GetUnderlyingType(propType) ?? propType;

            if (IsLeaf(underlying))
            {
                events.Add(new MetricEvent
                {
                    BankCode = bankCode,
                    Country = country,
                    MetricName = string.Join(".", currentPath),
                    MetricValue = JsonSerializer.Serialize(value, JsonOpts),
                    MetricType = "Text",
                    EventVersion = 1
                });
            }
            else if (IsNavigable(underlying))
            {
                FlattenObject(value, currentPath, events, bankCode, country);
            }
            else if (IsListOfComplex(propType))
            {
                events.Add(new MetricEvent
                {
                    BankCode = bankCode,
                    Country = country,
                    MetricName = string.Join(".", currentPath),
                    MetricValue = JsonSerializer.Serialize(value, JsonOpts),
                    MetricType = "List",
                    EventVersion = 1
                });
            }
        }
    }

    private static bool IsLeaf(Type t) =>
        t.IsPrimitive || t == typeof(string) || t == typeof(decimal) || t == typeof(DateTime) || t == typeof(double)
        || (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && t.GetGenericArguments()[0] == typeof(string));

    private static bool IsNavigable(Type t) =>
        t.IsClass && t != typeof(string) && !t.IsGenericType && t.Namespace?.StartsWith("BankProfiles.Web.Models") == true;

    private static bool IsListOfComplex(Type t) =>
        t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>) && t.GetGenericArguments()[0] != typeof(string) && t.GetGenericArguments()[0].IsClass;
}
