namespace LightJockey.Models;

/// <summary>
/// Event arguments containing FFT analysis results
/// </summary>
public class FFTResultEventArgs : EventArgs
{
    /// <summary>
    /// Gets the FFT magnitude spectrum (absolute values)
    /// </summary>
    public double[] Spectrum { get; }

    /// <summary>
    /// Gets the frequency resolution (Hz per bin)
    /// </summary>
    public double FrequencyResolution { get; }

    /// <summary>
    /// Gets the sample rate used for FFT
    /// </summary>
    public int SampleRate { get; }

    /// <summary>
    /// Gets the FFT size (number of bins)
    /// </summary>
    public int FFTSize { get; }

    /// <summary>
    /// Gets the timestamp when the FFT was computed
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Initializes a new instance of the FFTResultEventArgs class
    /// </summary>
    /// <param name="spectrum">FFT magnitude spectrum</param>
    /// <param name="sampleRate">Sample rate in Hz</param>
    /// <param name="fftSize">FFT size</param>
    public FFTResultEventArgs(double[] spectrum, int sampleRate, int fftSize)
    {
        Spectrum = spectrum ?? throw new ArgumentNullException(nameof(spectrum));
        SampleRate = sampleRate;
        FFTSize = fftSize;
        FrequencyResolution = (double)sampleRate / fftSize;
        Timestamp = DateTime.UtcNow;
    }
}
