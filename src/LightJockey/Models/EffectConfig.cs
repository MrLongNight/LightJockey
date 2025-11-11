namespace LightJockey.Models;

/// <summary>
/// Configuration settings for effect parameters
/// </summary>
public class EffectConfig
{
    /// <summary>
    /// Effect intensity (0.0 - 1.0)
    /// </summary>
    public double Intensity { get; init; } = 0.8;

    /// <summary>
    /// Effect speed multiplier (0.1 - 5.0)
    /// </summary>
    public double Speed { get; init; } = 1.0;

    /// <summary>
    /// Brightness level (0.0 - 1.0)
    /// </summary>
    public double Brightness { get; init; } = 0.8;

    /// <summary>
    /// Audio reactivity enabled
    /// </summary>
    public bool AudioReactive { get; init; } = true;

    /// <summary>
    /// Audio sensitivity (0.0 - 1.0)
    /// </summary>
    public double AudioSensitivity { get; init; } = 0.5;

    /// <summary>
    /// Smooth transitions enabled
    /// </summary>
    public bool SmoothTransitions { get; init; } = true;

    /// <summary>
    /// Transition duration in milliseconds
    /// </summary>
    public int TransitionDurationMs { get; init; } = 100;
}
