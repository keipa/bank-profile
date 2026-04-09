using System.Text.RegularExpressions;

namespace BankProfiles.Web.Services;

public static class ValidationHelper
{
    private static readonly Regex BankCodeRegex = new(@"^[a-zA-Z0-9\-_]+$", RegexOptions.Compiled);

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
}
