namespace LightJockey.Models;

/// <summary>
/// Event arguments containing spectral analysis data for frequency bands
/// </summary>
public class SpectralDataEventArgs : EventArgs
{
    /// <summary>
    /// Gets the low frequency band energy (20-250 Hz)
    /// </summary>
    public double LowFrequencyEnergy { get; }

    /// <summary>
    /// Gets the mid frequency band energy (250-2000 Hz)
    /// </summary>
    public double MidFrequencyEnergy { get; }

    /// <summary>
    /// Gets the high frequency band energy (2000-20000 Hz)
    /// </summary>
    public double HighFrequencyEnergy { get; }

    /// <summary>
    /// Gets the total spectral energy across all bands
    /// </summary>
    public double TotalEnergy { get; }

    /// <summary>
    /// Gets the timestamp when the spectral data was computed
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the SpectralDataEventArgs class
    /// </summary>
    /// <param name="lowFrequencyEnergy">Low frequency band energy</param>
    /// <param name="midFrequencyEnergy">Mid frequency band energy</param>
    /// <param name="highFrequencyEnergy">High frequency band energy</param>
    public SpectralDataEventArgs(double lowFrequencyEnergy, double midFrequencyEnergy, double highFrequencyEnergy)
    {
        LowFrequencyEnergy = lowFrequencyEnergy;
        MidFrequencyEnergy = midFrequencyEnergy;
        HighFrequencyEnergy = highFrequencyEnergy;
        TotalEnergy = lowFrequencyEnergy + midFrequencyEnergy + highFrequencyEnergy;
        Timestamp = DateTime.UtcNow;
    }
}
