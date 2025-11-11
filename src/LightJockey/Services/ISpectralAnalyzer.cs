using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Service for analyzing spectral content of audio across frequency bands
/// </summary>
public interface ISpectralAnalyzer : IDisposable
{
    /// <summary>
    /// Event raised when spectral analysis data is available
    /// </summary>
    event EventHandler<SpectralDataEventArgs>? SpectralDataAvailable;

    /// <summary>
    /// Analyzes FFT spectrum and computes energy across frequency bands
    /// </summary>
    /// <param name="spectrum">FFT magnitude spectrum</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    void AnalyzeSpectrum(double[] spectrum, int sampleRate);
}
