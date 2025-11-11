using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Service for detecting beats and estimating BPM in audio
/// </summary>
public interface IBeatDetector : IDisposable
{
    /// <summary>
    /// Event raised when a beat is detected
    /// </summary>
    event EventHandler<BeatDetectedEventArgs>? BeatDetected;

    /// <summary>
    /// Gets the current estimated BPM
    /// </summary>
    double CurrentBPM { get; }

    /// <summary>
    /// Processes audio energy to detect beats
    /// </summary>
    /// <param name="energy">Energy value from spectral analysis</param>
    void ProcessEnergy(double energy);

    /// <summary>
    /// Resets the beat detector state
    /// </summary>
    void Reset();
}
