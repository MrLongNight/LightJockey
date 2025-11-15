namespace LightJockey.Models;

/// <summary>
/// Configuration settings for LightJockey Entertainment V2 streaming
/// </summary>
public class LightJockeyEntertainmentConfig
{
    /// <summary>
    /// Target frame rate for streaming (default: 25 FPS)
    /// </summary>
    public int TargetFrameRate { get; init; } = 25;

    /// <summary>
    /// Whether to use color loop effect
    /// </summary>
    public bool UseColorLoop { get; init; } = false;

    /// <summary>
    /// Color loop speed (0.0 - 1.0)
    /// </summary>
    public double ColorLoopSpeed { get; init; } = 0.5;

    /// <summary>
    /// Whether to react to audio input
    /// </summary>
    public bool AudioReactive { get; init; } = true;

    /// <summary>
    /// Audio reactivity sensitivity (0.0 - 1.0)
    /// </summary>
    public double AudioSensitivity { get; init; } = 0.5;

    /// <summary>
    /// Minimum brightness level (0.0 - 1.0)
    /// </summary>
    public double MinBrightness { get; init; } = 0.1;

    /// <summary>
    /// Maximum brightness level (0.0 - 1.0)
    /// </summary>
    public double MaxBrightness { get; init; } = 1.0;

    /// <summary>
    /// A dictionary for storing sensitive or secure configuration values,
    /// such as API keys or authentication tokens.
    /// </summary>
    public Dictionary<string, string> SecureValues { get; set; } = new();
}
