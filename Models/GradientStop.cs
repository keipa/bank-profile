namespace BankProfiles.Web.Models
{
   /// <summary>
   /// Defines a color stop in a linear gradient.
   /// </summary>
   public class GradientStop
   {
      /// <summary>
      /// Position of the color stop in the gradient (e.g., "0%", "50%", "100%").
      /// Must be a percentage string between 0% and 100%.
      /// </summary>
      public required string Offset { get; set; }  // e.g., "0%", "100%"
    
      /// <summary>
      /// Hex color code for this stop (e.g., "#1a237e", "#3949ab").
      /// Must be a valid CSS hex color.
      /// </summary>
      public required string Color { get; set; }    // e.g., "#1a237e"
   }
}