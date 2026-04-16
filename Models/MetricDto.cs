namespace BankProfiles.Web.Models;

public enum MetricType
{
    Boolean,
    Numeric,
    Percentage,
    Currency,
    List,
    Text
}

public class MetricDto
{
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
        // Determine if metric should show alert/warning styling
        if (Label.Contains("Open Issues") && Value is int issues && issues > 10)
            return "alert";
        if (Label.Contains("Complaint Ratio") && Value is double ratio && ratio > 1.0)
            return "warning";
        if (Label.Contains("Red Flags") && Value is int flags && flags > 0)
            return "alert";
        if (Label.Contains("Remediation Days") && Value is int days && days > 7)
            return "warning";
        
        return "normal";
    }
}
