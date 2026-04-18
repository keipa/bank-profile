namespace BankProfiles.Web.Models
{
   /// <summary>
   /// Configuration for a single animated circle (ball).
   /// Coordinates are in SVG units (typically 0-600 for standard viewBox).
   /// </summary>
   public class BallConfig
   {
      /// <summary>
      /// X coordinate of the ball's center in SVG units.
      /// Range: 0-600 (for standard 600x600 viewBox)
      /// </summary>
      public int CenterX { get; set; }  // SVG X coordinate
    
      /// <summary>
      /// Y coordinate of the ball's center in SVG units.
      /// Range: 0-600 (for standard 600x600 viewBox)
      /// </summary>
      public int CenterY { get; set; }  // SVG Y coordinate
    
      /// <summary>
      /// Radius of the circle in SVG units.
      /// Recommended range: 60-150 for visual balance.
      /// </summary>
      public int Radius { get; set; }   // Circle radius
    
      /// <summary>
      /// Hex color code for the ball (e.g., "#ff6f00").
      /// Rendered with 60% opacity for a soft appearance.
      /// </summary>
      public required string Color { get; set; }  // Hex color
   }
}