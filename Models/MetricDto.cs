using System.Globalization;

namespace BankProfiles.Web.Models;

public class MetricDto
{
    public required string Key { get; set; }
    public required string Label { get; set; }
    public required object Value { get; set; }
    public required MetricType Type { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? Unit { get; set; }
    public bool IsBooleanTrue => Type == MetricType.Boolean && Value is bool boolValue && boolValue;
    public bool IsBooleanFalse => Type == MetricType.Boolean && Value is bool boolValue && !boolValue;
    
    public string FormattedValue => Type switch
    {
        MetricType.Boolean => Value is bool b ? (b ? "Yes" : "No") : "Unknown",
        MetricType.Percentage => Value is double d ? $"{d:F1}%" : Value?.ToString() ?? "N/A",
        MetricType.Currency => FormatCurrency(),
        MetricType.Numeric => Value?.ToString() ?? "0",
        MetricType.List => Value is List<string> list ? string.Join(", ", list) : Value?.ToString() ?? "",
        MetricType.Text => Value?.ToString() ?? "N/A",
        _ => Value?.ToString() ?? "N/A"
    };
    
    public string GetCssClass()
    {
        var baseClass = "metric-tile";
        if (Type == MetricType.Boolean)
        {
            return IsBooleanTrue ? $"{baseClass} boolean-true" : $"{baseClass} boolean-false";
        }
        return $"{baseClass} {Type.ToString().ToLowerInvariant()}";
    }
    
    private string FormatCurrency()
    {
        if (Value is double d)
        {
            var currencySymbol = Unit ?? "$";
            return $"{currencySymbol}{d:F2}";
        }
        return Value?.ToString() ?? "N/A";
    }
    
    public string GetAlertLevel()
    {
        if (!TryGetNumericValue(out var numericValue))
        {
            return "normal";
        }

        return Key switch
        {
            "openIssues" when numericValue > 10 => "alert",
            "complaintRatio" when numericValue > 1.0 => "warning",
            "redFlags" when numericValue > 0 => "alert",
            "avgRemediationDays" when numericValue > 7 => "warning",
            _ => "normal"
        };
    }

    private bool TryGetNumericValue(out double numericValue)
    {
        switch (Value)
        {
            case byte byteValue:
                numericValue = byteValue;
                return true;
            case sbyte sbyteValue:
                numericValue = sbyteValue;
                return true;
            case short shortValue:
                numericValue = shortValue;
                return true;
            case ushort ushortValue:
                numericValue = ushortValue;
                return true;
            case int intValue:
                numericValue = intValue;
                return true;
            case uint uintValue:
                numericValue = uintValue;
                return true;
            case long longValue:
                numericValue = longValue;
                return true;
            case ulong ulongValue:
                numericValue = ulongValue;
                return true;
            case float floatValue:
                numericValue = floatValue;
                return true;
            case double doubleValue:
                numericValue = doubleValue;
                return true;
            case decimal decimalValue:
                numericValue = (double)decimalValue;
                return true;
            case string textValue when double.TryParse(textValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedValue):
                numericValue = parsedValue;
                return true;
            default:
                numericValue = 0;
                return false;
        }
    }
}
