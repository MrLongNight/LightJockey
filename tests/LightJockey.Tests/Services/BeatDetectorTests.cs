using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services;

/// <summary>
/// Unit tests for BeatDetector
/// </summary>
public class BeatDetectorTests
{
    private readonly Mock<ILogger<BeatDetector>> _mockLogger;
    private readonly Mock<ILogger<SpectralAnalyzer>> _mockSpectralLogger;
    private readonly Mock<ILogger<FFTProcessor>> _mockFFTLogger;
    private readonly IFFTProcessor _fftProcessor;
    private readonly ISpectralAnalyzer _spectralAnalyzer;

    public BeatDetectorTests()
    {
        _mockLogger = new Mock<ILogger<BeatDetector>>();
        _mockSpectralLogger = new Mock<ILogger<SpectralAnalyzer>>();
        _mockFFTLogger = new Mock<ILogger<FFTProcessor>>();
        _fftProcessor = new FFTProcessor(_mockFFTLogger.Object, 2048);
        _spectralAnalyzer = new SpectralAnalyzer(_mockSpectralLogger.Object, _fftProcessor);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new BeatDetector(null!, _spectralAnalyzer));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullSpectralAnalyzer_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new BeatDetector(_mockLogger.Object, null!));
        Assert.Equal("spectralAnalyzer", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidParameters_InitializesWithZeroBPM()
    {
        // Arrange & Act
        using var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer);

        // Assert
        Assert.Equal(0, detector.CurrentBPM);
    }

    [Fact]
    public void ProcessEnergy_WithLowEnergy_DoesNotDetectBeat()
    {
        // Arrange
        using var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer, historySize: 10);
        var beatDetected = false;
        detector.BeatDetected += (s, e) => beatDetected = true;

        // Act - process constant low energy
        for (int i = 0; i < 20; i++)
        {
            detector.ProcessEnergy(0.1);
        }

        // Assert
        Assert.False(beatDetected);
    }

    [Fact]
    public void ProcessEnergy_WithEnergySpike_DetectsBeat()
    {
        // Arrange
        using var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer, historySize: 10);
        BeatDetectedEventArgs? eventArgs = null;
        detector.BeatDetected += (s, e) => eventArgs = e;

        // Act - build up history with low energy
        for (int i = 0; i < 15; i++)
        {
            detector.ProcessEnergy(1.0);
        }

        // Then spike the energy
        detector.ProcessEnergy(5.0);

        // Assert
        Assert.NotNull(eventArgs);
        Assert.True(eventArgs.Energy > 4.0);
    }

    [Fact]
    public void ProcessEnergy_WithMultipleBeats_CalculatesBPM()
    {
        // Arrange
        using var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer, historySize: 10);
        var beatCount = 0;
        BeatDetectedEventArgs? lastBeat = null;
        detector.BeatDetected += (s, e) =>
        {
            beatCount++;
            lastBeat = e;
        };

        // Act - simulate regular beats at 120 BPM (500ms intervals)
        for (int i = 0; i < 50; i++)
        {
            // Build history with low energy
            for (int j = 0; j < 10; j++)
            {
                detector.ProcessEnergy(1.0);
                Thread.Sleep(10);
            }

            // Beat spike
            detector.ProcessEnergy(5.0);
            Thread.Sleep(400); // Wait to exceed minimum interval
        }

        // Assert
        Assert.True(beatCount > 1, "Should detect multiple beats");
        Assert.NotNull(lastBeat);
        Assert.True(lastBeat.BPM > 0, "BPM should be calculated");
    }

    [Fact]
    public void ProcessEnergy_BeatConfidence_IsWithinValidRange()
    {
        // Arrange
        using var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer, historySize: 10);
        BeatDetectedEventArgs? eventArgs = null;
        detector.BeatDetected += (s, e) => eventArgs = e;

        // Act - build history and trigger beat
        for (int i = 0; i < 15; i++)
        {
            detector.ProcessEnergy(1.0);
        }
        detector.ProcessEnergy(5.0);

        // Assert
        Assert.NotNull(eventArgs);
        Assert.InRange(eventArgs.Confidence, 0.0, 1.0);
    }

    [Fact]
    public void ProcessEnergy_MinimumBeatInterval_PreventsRapidBeats()
    {
        // Arrange
        using var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer, historySize: 5);
        var beatCount = 0;
        detector.BeatDetected += (s, e) => beatCount++;

        // Act - build minimal history
        for (int i = 0; i < 10; i++)
        {
            detector.ProcessEnergy(1.0);
        }

        // Try to trigger multiple beats rapidly
        detector.ProcessEnergy(10.0);
        detector.ProcessEnergy(10.0);
        detector.ProcessEnergy(10.0);

        // Assert - should only detect one beat due to minimum interval
        Assert.Equal(1, beatCount);
    }

    [Fact]
    public void Reset_ClearsHistory_AndResetsBPM()
    {
        // Arrange
        using var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer, historySize: 10);
        
        // Build some history
        for (int i = 0; i < 15; i++)
        {
            detector.ProcessEnergy(1.0);
        }
        detector.ProcessEnergy(5.0);

        // Act
        detector.Reset();

        // Assert
        Assert.Equal(0, detector.CurrentBPM);
        
        // Verify no beat is detected immediately after reset
        var beatDetected = false;
        detector.BeatDetected += (s, e) => beatDetected = true;
        detector.ProcessEnergy(5.0);
        Assert.False(beatDetected); // No history, so no detection
    }

    [Fact]
    public void Dispose_AfterDisposal_ProcessEnergyThrowsObjectDisposedException()
    {
        // Arrange
        var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer);
        detector.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => detector.ProcessEnergy(1.0));
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer);

        // Act & Assert
        detector.Dispose();
        detector.Dispose(); // Should not throw
    }

    [Fact]
    public void Integration_FFTToSpectralToBeat_DetectsBeatsFromAudio()
    {
        // Arrange
        using var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer, historySize: 10);
        var beatCount = 0;
        detector.BeatDetected += (s, e) => beatCount++;

        // Act - simulate audio with beats (low frequency spikes)
        // Build up baseline
        for (int i = 0; i < 15; i++)
        {
            var samples = GenerateSineWave(80, 44100, 2048, amplitude: 0.2f);
            _fftProcessor.ProcessAudio(samples, 44100);
        }

        // Trigger beat with high amplitude
        var beatSamples = GenerateSineWave(80, 44100, 2048, amplitude: 1.0f);
        _fftProcessor.ProcessAudio(beatSamples, 44100);

        // Assert
        Assert.True(beatCount > 0, "Should detect at least one beat");
    }

    [Fact]
    public void ProcessEnergy_WithSilence_DoesNotDetectFalsePositives()
    {
        // Arrange
        using var detector = new BeatDetector(_mockLogger.Object, _spectralAnalyzer, historySize: 10);
        var beatDetected = false;
        detector.BeatDetected += (s, e) => beatDetected = true;

        // Act - process near-silence (very low energy)
        for (int i = 0; i < 50; i++)
        {
            detector.ProcessEnergy(0.0001);
        }

        // Even with small spike in silence
        detector.ProcessEnergy(0.0005);

        // Assert
        Assert.False(beatDetected, "Should not detect beats in silence");
    }

    // Helper method to generate a sine wave for testing
    private float[] GenerateSineWave(double frequency, int sampleRate, int sampleCount, float amplitude = 1.0f)
    {
        var samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            samples[i] = amplitude * (float)Math.Sin(2 * Math.PI * frequency * i / sampleRate);
        }
        return samples;
    }
}
