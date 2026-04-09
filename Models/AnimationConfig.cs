namespace BankProfiles.Web.Models;

/// <summary>
/// Configuration for animated SVG backgrounds on bank detail pages.
/// Defines gradients, floating elements (balls), and animation type.
/// </summary>
public class AnimationConfig
{
    /// <summary>
    /// List of gradient color stops for the background.
    /// Creates a smooth color transition from start to end.
    /// Minimum 2 stops required (0% and 100%).
    /// </summary>
    public List<GradientStop>? GradientStops { get; set; }
    
    /// <summary>
    /// List of floating circles (balls) to animate.
    /// Each ball can have different position, size, and color.
    /// Recommended: 2-4 balls for optimal visual effect.
    /// </summary>
    public List<BallConfig>? Balls { get; set; }
    
    /// <summary>
    /// Type of animation to apply to balls.
    /// Options: "pulse" (scale), "wave" (vertical movement), 
    ///          "rotate" (360° rotation), "float" (gentle motion)
    /// Default: "float" if not specified
    /// </summary>
    public string? AnimationType { get; set; }  // "pulse", "wave", "rotate", "float"
}

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
