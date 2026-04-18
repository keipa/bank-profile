using System.Globalization;

namespace BankProfiles.Web.Services
{
   public class CountryCodeMapperService : ICountryCodeMapperService
   {
      private static readonly Dictionary<string, string> CountryToIso2 = BuildCountryMap();
      private static readonly HashSet<string> Iso2Codes = CountryToIso2.Values.ToHashSet(StringComparer.OrdinalIgnoreCase);

      public bool TryGetIso2Code(string countryNameOrCode, out string iso2Code)
      {
         iso2Code = string.Empty;

         if (string.IsNullOrWhiteSpace(countryNameOrCode))
         {
            return false;
         }

         var normalized = NormalizeKey(countryNameOrCode);
         if (normalized.Length == 2 && normalized.All(char.IsLetter))
         {
            var candidate = normalized.ToUpperInvariant();
            var normalizedCandidate = candidate == "UK" ? "GB" : candidate;
            if (Iso2Codes.Contains(normalizedCandidate))
            {
               iso2Code = normalizedCandidate;
               return true;
            }

            return false;
         }

         if (CountryToIso2.TryGetValue(normalized, out var mappedCode))
         {
            iso2Code = mappedCode;
            return true;
         }

         return false;
      }

      private static Dictionary<string, string> BuildCountryMap()
      {
         var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

         foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
         {
            RegionInfo region;
            try
            {
               region = new RegionInfo(culture.Name);
            }
            catch (ArgumentException)
            {
               continue;
            }

            var iso2 = region.TwoLetterISORegionName.ToUpperInvariant();
            AddMapping(map, iso2, iso2);
            AddMapping(map, region.ThreeLetterISORegionName, iso2);
            AddMapping(map, region.EnglishName, iso2);
            AddMapping(map, region.NativeName, iso2);
            AddMapping(map, region.DisplayName, iso2);
         }

         // Common aliases and abbreviations frequently present in data feeds.
         AddMapping(map, "UK", "GB");
         AddMapping(map, "Great Britain", "GB");
         AddMapping(map, "USA", "US");
         AddMapping(map, "UAE", "AE");

         return map;
      }

      private static void AddMapping(Dictionary<string, string> map, string key, string iso2Code)
      {
         if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(iso2Code))
         {
            return;
         }

         map.TryAdd(NormalizeKey(key), iso2Code.ToUpperInvariant());
      }

      private static string NormalizeKey(string value)
      {
         return string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
      }
   }
}