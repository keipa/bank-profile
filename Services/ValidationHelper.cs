using System.Text.RegularExpressions;

namespace BankProfiles.Web.Services;

public static class ValidationHelper
{
    private static readonly Regex BankCodeRegex = new(@"^[a-zA-Z0-9\-_]+$", RegexOptions.Compiled);
    private static readonly Regex BankAssetPathRegex = new(
        @"^/images/banks/[A-Za-z0-9._/-]+\.(png|jpg|jpeg|svg|webp|ico)$",
        RegexOptions.Compiled);

    public static bool IsValidBankCode(string bankCode)
    {
        if (string.IsNullOrWhiteSpace(bankCode))
            return false;
        
        // Only allow alphanumeric, hyphens, underscores
        if (!BankCodeRegex.IsMatch(bankCode))
            return false;
        
        // Prevent path traversal
        if (bankCode.Contains("..") || bankCode.Contains("/") || bankCode.Contains("\\"))
            return false;
        
        return true;
    }

    public static bool IsValidBankAssetPath(string? assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
            return false;

        var trimmedPath = assetPath.Trim();

        if (trimmedPath.Contains("..", StringComparison.Ordinal)
            || trimmedPath.Contains("\\", StringComparison.Ordinal)
            || trimmedPath.Contains("//", StringComparison.Ordinal)
            || trimmedPath.Contains("?", StringComparison.Ordinal)
            || trimmedPath.Contains("#", StringComparison.Ordinal))
        {
            return false;
        }

        return BankAssetPathRegex.IsMatch(trimmedPath);
    }

    public static string? NormalizeBankAssetPath(string? assetPath)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
            return null;

        var trimmedPath = assetPath.Trim();
        return IsValidBankAssetPath(trimmedPath) ? trimmedPath : null;
    }
}
