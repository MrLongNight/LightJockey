namespace LightJockey.Models;

/// <summary>
/// Event arguments for beat detection events
/// </summary>
public class BeatDetectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the detected energy level at the beat
    /// </summary>
    public double Energy { get; }

    /// <summary>
    /// Gets the current estimated BPM (beats per minute)
    /// </summary>
    public double BPM { get; }

    /// <summary>
    /// Gets the confidence level of the beat detection (0.0 to 1.0)
    /// </summary>
    public double Confidence { get; }

    /// <summary>
    /// Gets the timestamp when the beat was detected
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the BeatDetectedEventArgs class
    /// </summary>
    /// <param name="energy">Energy level at the beat</param>
    /// <param name="bpm">Estimated BPM</param>
    /// <param name="confidence">Confidence level (0.0 to 1.0)</param>
    public BeatDetectedEventArgs(double energy, double bpm, double confidence)
    {
        Energy = energy;
        BPM = bpm;
        Confidence = Math.Clamp(confidence, 0.0, 1.0);
        Timestamp = DateTime.UtcNow;
    }
}
