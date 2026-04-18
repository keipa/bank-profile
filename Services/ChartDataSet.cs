namespace BankProfiles.Web.Services
{
   public class ChartDataSet
   {
      public string Label { get; set; } = string.Empty;
      public List<string> Labels { get; set; } = new();
      public List<decimal?> Data { get; set; } = new();
      public string BorderColor { get; set; } = string.Empty;
      public string BackgroundColor { get; set; } = string.Empty;
   }
}