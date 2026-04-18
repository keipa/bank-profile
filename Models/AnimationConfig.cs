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