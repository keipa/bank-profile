using System.Text.RegularExpressions;

// Validation Helper Test
var bankCodeRegex = new Regex(@"^[a-zA-Z0-9\-_]+$", RegexOptions.Compiled);

bool IsValidBankCode(string bankCode)
{
    if (string.IsNullOrWhiteSpace(bankCode))
        return false;
    
    if (!bankCodeRegex.IsMatch(bankCode))
        return false;
    
    if (bankCode.Contains("..") || bankCode.Contains("/") || bankCode.Contains("\\"))
        return false;
    
    return true;
}

void TestCode(string bankCode, bool expected, string description)
{
    bool result = IsValidBankCode(bankCode);
    string status = (result == expected) ? "✓ PASS" : "✗ FAIL";
    string displayCode = bankCode == null ? "<null>" : $"\"{bankCode}\"";
    Console.WriteLine($"{status}: {description} - {displayCode} => {result}");
}

Console.WriteLine("ValidationHelper Test Suite");
Console.WriteLine("============================\n");

// Test valid codes
TestCode("bank-alpha", true, "Valid bank code with hyphen");
TestCode("bank_test", true, "Valid bank code with underscore");
TestCode("BANK123", true, "Valid bank code with uppercase and numbers");

// Test invalid codes - path traversal
TestCode("../etc/passwd", false, "Path traversal with ../");
TestCode("..\\windows\\system32", false, "Path traversal with ..\\");
TestCode("bank/../other", false, "Path traversal in middle");

// Test invalid codes - path separators
TestCode("bank/code", false, "Forward slash");
TestCode("bank\\code", false, "Backslash");

// Test invalid codes - special characters
TestCode("bank code", false, "Space character");
TestCode("bank@code", false, "@ symbol");
TestCode("bank;code", false, "Semicolon");

// Test invalid codes - empty
TestCode("", false, "Empty string");
TestCode("   ", false, "Whitespace only");
TestCode(null, false, "Null value");

Console.WriteLine("\nAll tests completed!");
