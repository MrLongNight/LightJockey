using LightJockey.Models;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using Microsoft.Extensions.Logging;

namespace LightJockey.Services;

/// <summary>
/// Implementation of FFT processor for audio analysis
/// </summary>
public class FFTProcessor : IFFTProcessor
{
    private readonly ILogger<FFTProcessor> _logger;
    private readonly int _fftSize;
    private readonly double[] _window;
    private bool _disposed;

    /// <inheritdoc/>
    public event EventHandler<FFTResultEventArgs>? FFTResultAvailable;

    /// <inheritdoc/>
    public int FFTSize => _fftSize;

    /// <summary>
    /// Initializes a new instance of the FFTProcessor class
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="fftSize">FFT size (must be power of 2, default 2048)</param>
    public FFTProcessor(ILogger<FFTProcessor> logger, int fftSize = 2048)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate FFT size is power of 2
        if (fftSize <= 0 || (fftSize & (fftSize - 1)) != 0)
        {
            throw new ArgumentException("FFT size must be a power of 2", nameof(fftSize));
        }

        _fftSize = fftSize;
        _window = Window.Hann(_fftSize);

        _logger.LogDebug("FFTProcessor initialized with FFT size: {FFTSize}", _fftSize);
    }

    /// <inheritdoc/>
    public void ProcessAudio(float[] samples, int sampleRate)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(FFTProcessor));
        }

        if (samples == null || samples.Length == 0)
        {
            _logger.LogWarning("ProcessAudio called with null or empty samples");
            return;
        }

        try
        {
            // Need at least FFT size samples
            if (samples.Length < _fftSize)
            {
                _logger.LogDebug("Insufficient samples for FFT: {SampleCount} < {FFTSize}", samples.Length, _fftSize);
                return;
            }

            // Prepare complex array for FFT
            var complexSamples = new Complex32[_fftSize];

            // Apply windowing and convert to complex
            for (int i = 0; i < _fftSize; i++)
            {
                complexSamples[i] = new Complex32((float)(samples[i] * _window[i]), 0);
            }

            // Perform FFT
            Fourier.Forward(complexSamples, FourierOptions.Matlab);

            // Calculate magnitude spectrum (only need first half due to symmetry)
            var spectrum = new double[_fftSize / 2];
            for (int i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = complexSamples[i].Magnitude;
            }

            // Raise event with FFT results
            var eventArgs = new FFTResultEventArgs(spectrum, sampleRate, _fftSize);
            FFTResultAvailable?.Invoke(this, eventArgs);

            _logger.LogTrace("FFT processed successfully, spectrum size: {SpectrumSize}", spectrum.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing FFT");
            throw;
        }
    }

    /// <inheritdoc/>
    public int GetBinIndex(double frequency, int sampleRate)
    {
        if (frequency < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frequency), "Frequency must be non-negative");
        }

        double binWidth = (double)sampleRate / _fftSize;
        return (int)Math.Round(frequency / binWidth);
    }

    /// <inheritdoc/>
    public double GetFrequency(int binIndex, int sampleRate)
    {
        if (binIndex < 0 || binIndex >= _fftSize / 2)
        {
            throw new ArgumentOutOfRangeException(nameof(binIndex), "Bin index out of range");
        }

        double binWidth = (double)sampleRate / _fftSize;
        return binIndex * binWidth;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _logger.LogDebug("FFTProcessor disposed");
            _disposed = true;
        }
    }
}
