using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services;

/// <summary>
/// Unit tests for PerformanceMetricsService
/// </summary>
public class PerformanceMetricsServiceTests
{
    private readonly Mock<ILogger<PerformanceMetricsService>> _mockLogger;
    private readonly Mock<IMetricsService> _mockMetricsService;
    private readonly PerformanceMetricsService _service;

    public PerformanceMetricsServiceTests()
    {
        _mockLogger = new Mock<ILogger<PerformanceMetricsService>>();
        _mockMetricsService = new Mock<IMetricsService>();
        _service = new PerformanceMetricsService(_mockLogger.Object, _mockMetricsService.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PerformanceMetricsService(null!, _mockMetricsService.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidLogger_LogsDebugMessage()
    {
        // Arrange & Act
        var logger = new Mock<ILogger<PerformanceMetricsService>>();
        var metricsService = new Mock<IMetricsService>();
        var service = new PerformanceMetricsService(logger.Object, metricsService.Object);

        // Assert
        logger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("initialized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void StreamingFPS_InitiallyZero()
    {
        // Assert
        Assert.Equal(0, _service.StreamingFPS);
    }

    [Fact]
    public void AudioLatencyMs_InitiallyZero()
    {
        // Assert
        Assert.Equal(0, _service.AudioLatencyMs);
    }

    [Fact]
    public void FFTLatencyMs_InitiallyZero()
    {
        // Assert
        Assert.Equal(0, _service.FFTLatencyMs);
    }

    [Fact]
    public void EffectLatencyMs_InitiallyZero()
    {
        // Assert
        Assert.Equal(0, _service.EffectLatencyMs);
    }

    [Fact]
    public void TotalLatencyMs_InitiallyZero()
    {
        // Assert
        Assert.Equal(0, _service.TotalLatencyMs);
    }

    [Fact]
    public void FrameCount_InitiallyZero()
    {
        // Assert
        Assert.Equal(0, _service.FrameCount);
    }

    [Fact]
    public void RecordAudioLatency_WithValidLatency_UpdatesAudioLatency()
    {
        // Act
        _service.RecordAudioLatency(10.5);

        // Assert
        Assert.Equal(10.5, _service.AudioLatencyMs);
    }

    [Fact]
    public void RecordAudioLatency_WithNegativeLatency_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _service.RecordAudioLatency(-1));
        Assert.Equal("latencyMs", exception.ParamName);
    }

    [Fact]
    public void RecordFFTLatency_WithValidLatency_UpdatesFFTLatency()
    {
        // Act
        _service.RecordFFTLatency(5.2);

        // Assert
        Assert.Equal(5.2, _service.FFTLatencyMs);
    }

    [Fact]
    public void RecordFFTLatency_WithNegativeLatency_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _service.RecordFFTLatency(-1));
        Assert.Equal("latencyMs", exception.ParamName);
    }

    [Fact]
    public void RecordEffectLatency_WithValidLatency_UpdatesEffectLatency()
    {
        // Act
        _service.RecordEffectLatency(3.8);

        // Assert
        Assert.Equal(3.8, _service.EffectLatencyMs);
    }

    [Fact]
    public void RecordEffectLatency_WithNegativeLatency_ThrowsArgumentException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _service.RecordEffectLatency(-1));
        Assert.Equal("latencyMs", exception.ParamName);
    }

    [Fact]
    public void TotalLatencyMs_SumsAllLatencies()
    {
        // Arrange
        _service.RecordAudioLatency(10.0);
        _service.RecordFFTLatency(5.0);
        _service.RecordEffectLatency(3.0);

        // Act
        var totalLatency = _service.TotalLatencyMs;

        // Assert
        Assert.Equal(18.0, totalLatency);
    }

    [Fact]
    public void RecordAudioLatency_CalculatesMovingAverage()
    {
        // Act
        _service.RecordAudioLatency(10.0);
        _service.RecordAudioLatency(20.0);
        _service.RecordAudioLatency(30.0);

        // Assert
        Assert.Equal(20.0, _service.AudioLatencyMs); // (10 + 20 + 30) / 3 = 20
    }

    [Fact]
    public void RecordFFTLatency_CalculatesMovingAverage()
    {
        // Act
        _service.RecordFFTLatency(5.0);
        _service.RecordFFTLatency(7.0);

        // Assert
        Assert.Equal(6.0, _service.FFTLatencyMs); // (5 + 7) / 2 = 6
    }

    [Fact]
    public void RecordEffectLatency_CalculatesMovingAverage()
    {
        // Act
        _service.RecordEffectLatency(2.0);
        _service.RecordEffectLatency(4.0);
        _service.RecordEffectLatency(6.0);

        // Assert
        Assert.Equal(4.0, _service.EffectLatencyMs); // (2 + 4 + 6) / 3 = 4
    }

    [Fact]
    public void StartStreamingFrame_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _service.StartStreamingFrame();
    }

    [Fact]
    public void EndStreamingFrame_IncrementsFrameCount()
    {
        // Act
        _service.StartStreamingFrame();
        _service.EndStreamingFrame();

        // Assert
        Assert.Equal(1, _service.FrameCount);
    }

    [Fact]
    public void EndStreamingFrame_MultipleTimes_IncrementsFrameCount()
    {
        // Act
        for (int i = 0; i < 5; i++)
        {
            _service.StartStreamingFrame();
            _service.EndStreamingFrame();
        }

        // Assert
        Assert.Equal(5, _service.FrameCount);
    }

    [Fact]
    public async Task EndStreamingFrame_CalculatesFPS()
    {
        // Act - Process frames with small delay
        for (int i = 0; i < 10; i++)
        {
            _service.StartStreamingFrame();
            await Task.Delay(1); // Small delay between frames
            _service.EndStreamingFrame();
        }

        // Assert - FPS should be calculated after 10 frames
        Assert.True(_service.StreamingFPS > 0, "FPS should be calculated after 10 frames");
    }

    [Fact]
    public void GetMetrics_ReturnsMetricsSnapshot()
    {
        // Arrange
        _service.RecordAudioLatency(10.0);
        _service.RecordFFTLatency(5.0);
        _service.RecordEffectLatency(3.0);
        _service.StartStreamingFrame();
        _service.EndStreamingFrame();

        // Act
        var metrics = _service.GetMetrics();

        // Assert
        Assert.NotNull(metrics);
        Assert.Equal(10.0, metrics.AudioLatencyMs);
        Assert.Equal(5.0, metrics.FFTLatencyMs);
        Assert.Equal(3.0, metrics.EffectLatencyMs);
        Assert.Equal(18.0, metrics.TotalLatencyMs);
        Assert.Equal(1, metrics.FrameCount);
        Assert.True((DateTime.UtcNow - metrics.Timestamp).TotalSeconds < 1);
    }

    [Fact]
    public void Reset_ClearsAllMetrics()
    {
        // Arrange
        _service.RecordAudioLatency(10.0);
        _service.RecordFFTLatency(5.0);
        _service.RecordEffectLatency(3.0);
        _service.StartStreamingFrame();
        _service.EndStreamingFrame();

        // Act
        _service.Reset();

        // Assert
        Assert.Equal(0, _service.StreamingFPS);
        Assert.Equal(0, _service.AudioLatencyMs);
        Assert.Equal(0, _service.FFTLatencyMs);
        Assert.Equal(0, _service.EffectLatencyMs);
        Assert.Equal(0, _service.TotalLatencyMs);
        Assert.Equal(0, _service.FrameCount);
    }

    [Fact]
    public void Reset_LogsDebugMessage()
    {
        // Act
        _service.Reset();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("reset")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ThreadSafety_ConcurrentOperations_DoNotThrow()
    {
        // Arrange
        var tasks = new List<Task>();

        // Act - Perform concurrent operations
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                _service.RecordAudioLatency(10.0);
                _service.RecordFFTLatency(5.0);
                _service.RecordEffectLatency(3.0);
                _service.StartStreamingFrame();
                _service.EndStreamingFrame();
                var _ = _service.GetMetrics();
            }));
        }

        // Assert - should not throw
        var exception = Record.Exception(() => Task.WaitAll(tasks.ToArray()));
        Assert.Null(exception);
    }

    [Fact]
    public void MovingAverage_LimitedToWindowSize()
    {
        // Act - Add more than window size (30) samples
        for (int i = 1; i <= 35; i++)
        {
            _service.RecordAudioLatency(i);
        }

        // Assert - Should only average last 30 samples (6-35)
        var expectedAverage = Enumerable.Range(6, 30).Average();
        Assert.Equal(expectedAverage, _service.AudioLatencyMs);
    }
}
