using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services;

/// <summary>
/// Unit tests for FFTProcessor
/// </summary>
public class FFTProcessorTests
{
    private readonly Mock<ILogger<FFTProcessor>> _mockLogger;

    public FFTProcessorTests()
    {
        _mockLogger = new Mock<ILogger<FFTProcessor>>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new FFTProcessor(null!));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidFFTSize_ThrowsArgumentException()
    {
        // Act & Assert - not power of 2
        Assert.Throws<ArgumentException>(() => new FFTProcessor(_mockLogger.Object, 1000));
        
        // Act & Assert - zero
        Assert.Throws<ArgumentException>(() => new FFTProcessor(_mockLogger.Object, 0));
        
        // Act & Assert - negative
        Assert.Throws<ArgumentException>(() => new FFTProcessor(_mockLogger.Object, -1024));
    }

    [Fact]
    public void Constructor_WithValidFFTSize_SetsProperty()
    {
        // Arrange & Act
        var processor = new FFTProcessor(_mockLogger.Object, 2048);

        // Assert
        Assert.Equal(2048, processor.FFTSize);
    }

    [Theory]
    [InlineData(1024)]
    [InlineData(2048)]
    [InlineData(4096)]
    public void Constructor_WithPowerOfTwo_Succeeds(int fftSize)
    {
        // Act
        var processor = new FFTProcessor(_mockLogger.Object, fftSize);

        // Assert
        Assert.Equal(fftSize, processor.FFTSize);
    }

    [Fact]
    public void ProcessAudio_WithNullSamples_DoesNotThrow()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 1024);

        // Act & Assert - should log warning but not throw
        processor.ProcessAudio(null!, 44100);
    }

    [Fact]
    public void ProcessAudio_WithEmptySamples_DoesNotThrow()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 1024);

        // Act & Assert - should log warning but not throw
        processor.ProcessAudio(Array.Empty<float>(), 44100);
    }

    [Fact]
    public void ProcessAudio_WithInsufficientSamples_DoesNotRaiseEvent()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 1024);
        var eventRaised = false;
        processor.FFTResultAvailable += (s, e) => eventRaised = true;

        // Act - provide fewer samples than FFT size
        var samples = new float[512];
        processor.ProcessAudio(samples, 44100);

        // Assert
        Assert.False(eventRaised);
    }

    [Fact]
    public void ProcessAudio_WithValidSamples_RaisesFFTResultEvent()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 1024);
        FFTResultEventArgs? eventArgs = null;
        processor.FFTResultAvailable += (s, e) => eventArgs = e;

        // Act - provide valid samples (sine wave at 440 Hz)
        var samples = GenerateSineWave(440, 44100, 1024);
        processor.ProcessAudio(samples, 44100);

        // Assert
        Assert.NotNull(eventArgs);
        Assert.Equal(44100, eventArgs.SampleRate);
        Assert.Equal(1024, eventArgs.FFTSize);
        Assert.Equal(512, eventArgs.Spectrum.Length); // Half of FFT size
        Assert.True(eventArgs.FrequencyResolution > 0);
    }

    [Fact]
    public void ProcessAudio_WithSineWave_DetectsCorrectFrequency()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 2048);
        FFTResultEventArgs? eventArgs = null;
        processor.FFTResultAvailable += (s, e) => eventArgs = e;

        // Act - generate 440 Hz sine wave
        var samples = GenerateSineWave(440, 44100, 2048);
        processor.ProcessAudio(samples, 44100);

        // Assert
        Assert.NotNull(eventArgs);
        
        // Find peak in spectrum (should be near 440 Hz)
        var spectrum = eventArgs.Spectrum;
        int peakIndex = 0;
        double peakValue = 0;
        for (int i = 0; i < spectrum.Length; i++)
        {
            if (spectrum[i] > peakValue)
            {
                peakValue = spectrum[i];
                peakIndex = i;
            }
        }

        double detectedFreq = processor.GetFrequency(peakIndex, 44100);
        
        // Should be within 50 Hz of target frequency
        Assert.InRange(detectedFreq, 390, 490);
    }

    [Fact]
    public void GetBinIndex_WithValidFrequency_ReturnsCorrectBin()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 2048);

        // Act
        int binIndex = processor.GetBinIndex(1000, 44100);

        // Assert - 1000 Hz with 44100 Hz sample rate and 2048 FFT size
        // Bin width = 44100 / 2048 ≈ 21.5 Hz
        // 1000 / 21.5 ≈ 46
        Assert.InRange(binIndex, 45, 47);
    }

    [Fact]
    public void GetBinIndex_WithNegativeFrequency_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 1024);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => processor.GetBinIndex(-100, 44100));
    }

    [Fact]
    public void GetFrequency_WithValidBinIndex_ReturnsCorrectFrequency()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 2048);

        // Act
        double frequency = processor.GetFrequency(46, 44100);

        // Assert - bin 46 * (44100 / 2048) ≈ 990 Hz
        Assert.InRange(frequency, 980, 1000);
    }

    [Fact]
    public void GetFrequency_WithInvalidBinIndex_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 1024);

        // Act & Assert - negative bin
        Assert.Throws<ArgumentOutOfRangeException>(() => processor.GetFrequency(-1, 44100));
        
        // Act & Assert - bin too large
        Assert.Throws<ArgumentOutOfRangeException>(() => processor.GetFrequency(1024, 44100));
    }

    [Fact]
    public void Dispose_AfterDisposal_ProcessAudioThrowsObjectDisposedException()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 1024);
        processor.Dispose();

        // Act & Assert
        var samples = new float[1024];
        Assert.Throws<ObjectDisposedException>(() => processor.ProcessAudio(samples, 44100));
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var processor = new FFTProcessor(_mockLogger.Object, 1024);

        // Act & Assert
        processor.Dispose();
        processor.Dispose(); // Should not throw
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
