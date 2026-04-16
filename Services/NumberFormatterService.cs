namespace BankProfiles.Web.Services;

public class NumberFormatterService : INumberFormatterService
{
    public string FormatShort(long number)
    {
        return FormatShort((decimal)number);
    }

    public string FormatShort(decimal number)
    {
        var absNumber = Math.Abs(number);
        var sign = number < 0 ? "-" : "";

        if (absNumber >= 1_000_000_000)
        {
            // Billions: 1.2B, 24B
            var billions = absNumber / 1_000_000_000;
            return $"{sign}{billions:0.#}B";
        }
        else if (absNumber >= 1_000_000)
        {
            // Millions: 1.5M, 24M
            var millions = absNumber / 1_000_000;
            return $"{sign}{millions:0.#}M";
        }
        else if (absNumber >= 1_000)
        {
            // Thousands: 185K, 1K (no decimals)
            var thousands = absNumber / 1_000;
            return $"{sign}{(int)thousands}K";
        }
        else
        {
            // Less than 1000: show as-is
            return $"{sign}{(int)absNumber}";
        }
    }

    public string FormatWithSuffix(decimal number, string suffix)
    {
        var formatted = FormatShort(number);
        return string.IsNullOrEmpty(suffix) ? formatted : $"{formatted} {suffix}";
    }
}
