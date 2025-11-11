using LightJockey.Models;

namespace LightJockey.Services;

/// <summary>
/// Service for performing FFT (Fast Fourier Transform) on audio data
/// </summary>
public interface IFFTProcessor : IDisposable
{
    /// <summary>
    /// Event raised when FFT results are available
    /// </summary>
    event EventHandler<FFTResultEventArgs>? FFTResultAvailable;

    /// <summary>
    /// Gets the FFT size (number of samples processed)
    /// </summary>
    int FFTSize { get; }

    /// <summary>
    /// Processes audio samples and computes FFT
    /// </summary>
    /// <param name="samples">Audio samples to process</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    void ProcessAudio(float[] samples, int sampleRate);

    /// <summary>
    /// Gets the frequency bin index for a given frequency
    /// </summary>
    /// <param name="frequency">Frequency in Hz</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    /// <returns>Bin index</returns>
    int GetBinIndex(double frequency, int sampleRate);

    /// <summary>
    /// Gets the frequency for a given bin index
    /// </summary>
    /// <param name="binIndex">Bin index</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    /// <returns>Frequency in Hz</returns>
    double GetFrequency(int binIndex, int sampleRate);
}
