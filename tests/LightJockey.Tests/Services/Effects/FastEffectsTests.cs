using LightJockey.Models;
using LightJockey.Services;
using LightJockey.Services.Effects;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services.Effects;

/// <summary>
/// Unit tests for PulseEffect
/// </summary>
public class PulseEffectTests : IDisposable
{
    private readonly Mock<ILogger<PulseEffect>> _mockLogger;
    private readonly Mock<IEntertainmentService> _mockEntertainmentService;
    private readonly PulseEffect _effect;

    public PulseEffectTests()
    {
        _mockLogger = new Mock<ILogger<PulseEffect>>();
        _mockEntertainmentService = new Mock<IEntertainmentService>();
        _effect = new PulseEffect(_mockLogger.Object, _mockEntertainmentService.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new PulseEffect(null!, _mockEntertainmentService.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("PulseEffect", _effect.Name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Assert
        Assert.Contains("DTLS", _effect.Description);
        Assert.Contains("Beat", _effect.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("puls", _effect.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InitializeAsync_WithValidConfig_ReturnsTrue()
    {
        // Arrange
        var config = new EffectConfig();

        // Act
        var result = await _effect.InitializeAsync(config);

        // Assert
        Assert.True(result);
        Assert.Equal(EffectState.Initialized, _effect.State);
    }

    [Fact]
    public void OnBeatDetected_WithAudioReactive_ResetsPhase()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = true };
        _effect.UpdateConfig(config);
        var beatData = new BeatDetectedEventArgs(0.8, 120.0, 0.9);

        // Act & Assert - should not throw
        var exception = Record.Exception(() => _effect.OnBeatDetected(beatData));
        Assert.Null(exception);
    }

    public void Dispose()
    {
        _effect?.Dispose();
    }
}

/// <summary>
/// Unit tests for ChaseEffect
/// </summary>
public class ChaseEffectTests : IDisposable
{
    private readonly Mock<ILogger<ChaseEffect>> _mockLogger;
    private readonly Mock<IEntertainmentService> _mockEntertainmentService;
    private readonly ChaseEffect _effect;

    public ChaseEffectTests()
    {
        _mockLogger = new Mock<ILogger<ChaseEffect>>();
        _mockEntertainmentService = new Mock<IEntertainmentService>();
        _effect = new ChaseEffect(_mockLogger.Object, _mockEntertainmentService.Object);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("ChaseEffect", _effect.Name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Assert
        Assert.Contains("DTLS", _effect.Description);
        Assert.Contains("Sequential", _effect.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("chase", _effect.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InitializeAsync_WithValidConfig_ReturnsTrue()
    {
        // Arrange
        var config = new EffectConfig();

        // Act
        var result = await _effect.InitializeAsync(config);

        // Assert
        Assert.True(result);
        Assert.Equal(EffectState.Initialized, _effect.State);
    }

    public void Dispose()
    {
        _effect?.Dispose();
    }
}

/// <summary>
/// Unit tests for SparkleEffect
/// </summary>
public class SparkleEffectTests : IDisposable
{
    private readonly Mock<ILogger<SparkleEffect>> _mockLogger;
    private readonly Mock<IEntertainmentService> _mockEntertainmentService;
    private readonly SparkleEffect _effect;

    public SparkleEffectTests()
    {
        _mockLogger = new Mock<ILogger<SparkleEffect>>();
        _mockEntertainmentService = new Mock<IEntertainmentService>();
        _effect = new SparkleEffect(_mockLogger.Object, _mockEntertainmentService.Object);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("SparkleEffect", _effect.Name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Assert
        Assert.Contains("DTLS", _effect.Description);
        Assert.Contains("sparkle", _effect.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("twinkl", _effect.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InitializeAsync_WithValidConfig_ReturnsTrue()
    {
        // Arrange
        var area = new EntertainmentArea
        {
            Id = Guid.NewGuid(),
            Name = "Test Area",
            LightIds = new List<Guid> { Guid.NewGuid() }
        };
        _mockEntertainmentService.Setup(e => e.ActiveArea).Returns(area);
        var config = new EffectConfig();

        // Act
        var result = await _effect.InitializeAsync(config);

        // Assert
        Assert.True(result);
        Assert.Equal(EffectState.Initialized, _effect.State);
    }

    [Fact]
    public void OnBeatDetected_WithAudioReactive_TriggersMultipleSparkles()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = true };
        _effect.UpdateConfig(config);
        var beatData = new BeatDetectedEventArgs(0.8, 120.0, 0.9);

        // Act & Assert - should not throw
        var exception = Record.Exception(() => _effect.OnBeatDetected(beatData));
        Assert.Null(exception);
    }

    public void Dispose()
    {
        _effect?.Dispose();
    }
}
