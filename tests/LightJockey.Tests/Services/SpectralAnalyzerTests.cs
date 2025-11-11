using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services;

/// <summary>
/// Unit tests for SpectralAnalyzer
/// </summary>
public class SpectralAnalyzerTests
{
    private readonly Mock<ILogger<SpectralAnalyzer>> _mockLogger;
    private readonly Mock<ILogger<FFTProcessor>> _mockFFTLogger;
    private readonly IFFTProcessor _fftProcessor;

    public SpectralAnalyzerTests()
    {
        _mockLogger = new Mock<ILogger<SpectralAnalyzer>>();
        _mockFFTLogger = new Mock<ILogger<FFTProcessor>>();
        _fftProcessor = new FFTProcessor(_mockFFTLogger.Object, 2048);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new SpectralAnalyzer(null!, _fftProcessor));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullFFTProcessor_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new SpectralAnalyzer(_mockLogger.Object, null!));
        Assert.Equal("fftProcessor", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_SubscribesToFFTEvents()
    {
        // Arrange & Act
        using var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);

        // Assert - verify FFT event is subscribed by triggering it
        SpectralDataEventArgs? eventArgs = null;
        analyzer.SpectralDataAvailable += (s, e) => eventArgs = e;

        var samples = GenerateSineWave(100, 44100, 2048); // Low frequency
        _fftProcessor.ProcessAudio(samples, 44100);

        Assert.NotNull(eventArgs);
    }

    [Fact]
    public void AnalyzeSpectrum_WithNullSpectrum_DoesNotThrow()
    {
        // Arrange
        using var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);

        // Act & Assert - should log warning but not throw
        analyzer.AnalyzeSpectrum(null!, 44100);
    }

    [Fact]
    public void AnalyzeSpectrum_WithEmptySpectrum_DoesNotThrow()
    {
        // Arrange
        using var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);

        // Act & Assert - should log warning but not throw
        analyzer.AnalyzeSpectrum(Array.Empty<double>(), 44100);
    }

    [Fact]
    public void AnalyzeSpectrum_WithValidSpectrum_RaisesSpectralDataEvent()
    {
        // Arrange
        using var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);
        SpectralDataEventArgs? eventArgs = null;
        analyzer.SpectralDataAvailable += (s, e) => eventArgs = e;

        // Act - create a mock spectrum
        var spectrum = new double[1024];
        for (int i = 0; i < spectrum.Length; i++)
        {
            spectrum[i] = 1.0; // Uniform energy
        }
        analyzer.AnalyzeSpectrum(spectrum, 44100);

        // Assert
        Assert.NotNull(eventArgs);
        Assert.True(eventArgs.LowFrequencyEnergy > 0);
        Assert.True(eventArgs.MidFrequencyEnergy > 0);
        Assert.True(eventArgs.HighFrequencyEnergy > 0);
        Assert.True(eventArgs.TotalEnergy > 0);
    }

    [Fact]
    public void AnalyzeSpectrum_WithLowFrequencyContent_HasHigherLowBandEnergy()
    {
        // Arrange
        using var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);
        SpectralDataEventArgs? eventArgs = null;
        analyzer.SpectralDataAvailable += (s, e) => eventArgs = e;

        // Act - generate low frequency content (100 Hz sine wave)
        var samples = GenerateSineWave(100, 44100, 2048);
        _fftProcessor.ProcessAudio(samples, 44100);

        // Assert
        Assert.NotNull(eventArgs);
        Assert.True(eventArgs.LowFrequencyEnergy > eventArgs.MidFrequencyEnergy);
        Assert.True(eventArgs.LowFrequencyEnergy > eventArgs.HighFrequencyEnergy);
    }

    [Fact]
    public void AnalyzeSpectrum_WithMidFrequencyContent_HasHigherMidBandEnergy()
    {
        // Arrange
        using var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);
        SpectralDataEventArgs? eventArgs = null;
        analyzer.SpectralDataAvailable += (s, e) => eventArgs = e;

        // Act - generate mid frequency content (1000 Hz sine wave)
        var samples = GenerateSineWave(1000, 44100, 2048);
        _fftProcessor.ProcessAudio(samples, 44100);

        // Assert
        Assert.NotNull(eventArgs);
        Assert.True(eventArgs.MidFrequencyEnergy > eventArgs.LowFrequencyEnergy);
        Assert.True(eventArgs.MidFrequencyEnergy > eventArgs.HighFrequencyEnergy);
    }

    [Fact]
    public void AnalyzeSpectrum_WithHighFrequencyContent_HasHigherHighBandEnergy()
    {
        // Arrange
        using var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);
        SpectralDataEventArgs? eventArgs = null;
        analyzer.SpectralDataAvailable += (s, e) => eventArgs = e;

        // Act - generate high frequency content (5000 Hz sine wave)
        var samples = GenerateSineWave(5000, 44100, 2048);
        _fftProcessor.ProcessAudio(samples, 44100);

        // Assert
        Assert.NotNull(eventArgs);
        Assert.True(eventArgs.HighFrequencyEnergy > eventArgs.LowFrequencyEnergy);
        Assert.True(eventArgs.HighFrequencyEnergy > eventArgs.MidFrequencyEnergy);
    }

    [Fact]
    public void AnalyzeSpectrum_TotalEnergy_IsSumOfAllBands()
    {
        // Arrange
        using var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);
        SpectralDataEventArgs? eventArgs = null;
        analyzer.SpectralDataAvailable += (s, e) => eventArgs = e;

        // Act
        var samples = GenerateSineWave(440, 44100, 2048);
        _fftProcessor.ProcessAudio(samples, 44100);

        // Assert
        Assert.NotNull(eventArgs);
        double expectedTotal = eventArgs.LowFrequencyEnergy + 
                              eventArgs.MidFrequencyEnergy + 
                              eventArgs.HighFrequencyEnergy;
        Assert.Equal(expectedTotal, eventArgs.TotalEnergy, 0.0001);
    }

    [Fact]
    public void Dispose_AfterDisposal_AnalyzeSpectrumThrowsObjectDisposedException()
    {
        // Arrange
        var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);
        analyzer.Dispose();

        // Act & Assert
        var spectrum = new double[1024];
        Assert.Throws<ObjectDisposedException>(() => analyzer.AnalyzeSpectrum(spectrum, 44100));
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);

        // Act & Assert
        analyzer.Dispose();
        analyzer.Dispose(); // Should not throw
    }

    [Fact]
    public void Dispose_UnsubscribesFromFFTEvents()
    {
        // Arrange
        var analyzer = new SpectralAnalyzer(_mockLogger.Object, _fftProcessor);
        SpectralDataEventArgs? eventArgs = null;
        analyzer.SpectralDataAvailable += (s, e) => eventArgs = e;

        // Act - dispose analyzer
        analyzer.Dispose();

        // Process audio after disposal
        var samples = GenerateSineWave(440, 44100, 2048);
        _fftProcessor.ProcessAudio(samples, 44100);

        // Assert - spectral analyzer should not have received event
        Assert.Null(eventArgs);
    }

    // Helper method to generate a sine wave for testing
    private float[] GenerateSineWave(double frequency, int sampleRate, int sampleCount)
    {
        var samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = (float)Math.Sin(2 * Math.PI * frequency * i / sampleRate);
        }
        return samples;
    }
}
