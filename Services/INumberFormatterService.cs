namespace BankProfiles.Web.Services;

/// <summary>
/// Service for formatting large numbers into human-readable short format
/// </summary>
public interface INumberFormatterService
{
    /// <summary>
    /// Formats a number to short format with suffix (K, M, B)
    /// Examples: 185000 → "185K", 1500000 → "1.5M", 24000000 → "24M"
    /// </summary>
    string FormatShort(long number);
    
    /// <summary>
    /// Formats a decimal number to short format with suffix
    /// </summary>
    string FormatShort(decimal number);
    
    /// <summary>
    /// Formats a number with a custom suffix
    /// </summary>
    string FormatWithSuffix(decimal number, string suffix);
}
