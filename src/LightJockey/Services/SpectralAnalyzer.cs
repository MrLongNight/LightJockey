using LightJockey.Models;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services;

/// <summary>
/// Implementation of spectral analyzer for frequency band analysis
/// </summary>
public class SpectralAnalyzer : ISpectralAnalyzer
{
    private readonly ILogger<SpectralAnalyzer> _logger;
    private readonly IFFTProcessor _fftProcessor;
    private bool _disposed;

    // Frequency band boundaries (in Hz)
    private const double LowFreqMin = 20;
    private const double LowFreqMax = 250;
    private const double MidFreqMin = 250;
    private const double MidFreqMax = 2000;
    private const double HighFreqMin = 2000;
    private const double HighFreqMax = 20000;

    /// <inheritdoc/>
    public event EventHandler<SpectralDataEventArgs>? SpectralDataAvailable;

    /// <summary>
    /// Initializes a new instance of the SpectralAnalyzer class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="fftProcessor">FFT processor instance</param>
    public SpectralAnalyzer(ILogger<SpectralAnalyzer> logger, IFFTProcessor fftProcessor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fftProcessor = fftProcessor ?? throw new ArgumentNullException(nameof(fftProcessor));

        // Subscribe to FFT results
        _fftProcessor.FFTResultAvailable += OnFFTResultAvailable;

        _logger.LogDebug("SpectralAnalyzer initialized");
    }

    private void OnFFTResultAvailable(object? sender, FFTResultEventArgs e)
    {
        AnalyzeSpectrum(e.Spectrum, e.SampleRate);
    }

    /// <inheritdoc/>
    public void AnalyzeSpectrum(double[] spectrum, int sampleRate)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SpectralAnalyzer));
        }

        if (spectrum == null || spectrum.Length == 0)
        {
            _logger.LogWarning("AnalyzeSpectrum called with null or empty spectrum");
            return;
        }

        try
        {
            // Calculate bin indices for frequency bands
            int lowStartBin = _fftProcessor.GetBinIndex(LowFreqMin, sampleRate);
            int lowEndBin = _fftProcessor.GetBinIndex(LowFreqMax, sampleRate);
            int midStartBin = _fftProcessor.GetBinIndex(MidFreqMin, sampleRate);
            int midEndBin = _fftProcessor.GetBinIndex(MidFreqMax, sampleRate);
            int highStartBin = _fftProcessor.GetBinIndex(HighFreqMin, sampleRate);
            int highEndBin = Math.Min(_fftProcessor.GetBinIndex(HighFreqMax, sampleRate), spectrum.Length - 1);

            // Calculate energy in each band (sum of squared magnitudes)
            double lowEnergy = CalculateBandEnergy(spectrum, lowStartBin, lowEndBin);
            double midEnergy = CalculateBandEnergy(spectrum, midStartBin, midEndBin);
            double highEnergy = CalculateBandEnergy(spectrum, highStartBin, highEndBin);

            // Raise event with spectral data
            var eventArgs = new SpectralDataEventArgs(lowEnergy, midEnergy, highEnergy);
            SpectralDataAvailable?.Invoke(this, eventArgs);

            _logger.LogTrace("Spectral analysis completed - Low: {Low:F2}, Mid: {Mid:F2}, High: {High:F2}",
                lowEnergy, midEnergy, highEnergy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing spectrum");
            throw;
        }
    }

    private double CalculateBandEnergy(double[] spectrum, int startBin, int endBin)
    {
        double energy = 0;
        
        // Ensure bins are within valid range
        startBin = Math.Max(0, startBin);
        endBin = Math.Min(spectrum.Length - 1, endBin);

        for (int i = startBin; i <= endBin; i++)
        {
            energy += spectrum[i] * spectrum[i];
        }

        // Normalize by number of bins to get average energy
        int binCount = endBin - startBin + 1;
        return binCount > 0 ? energy / binCount : 0;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _fftProcessor.FFTResultAvailable -= OnFFTResultAvailable;
            _logger.LogDebug("SpectralAnalyzer disposed");
            _disposed = true;
        }
    }
}
