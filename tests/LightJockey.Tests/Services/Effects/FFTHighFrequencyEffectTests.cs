using LightJockey.Models;
using LightJockey.Services;
using LightJockey.Services.Effects;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services.Effects;

/// <summary>
/// Unit tests for FFTHighFrequencyEffect
/// </summary>
public class FFTHighFrequencyEffectTests : IDisposable
{
    private readonly Mock<ILogger<FFTHighFrequencyEffect>> _mockLogger;
    private readonly Mock<IEntertainmentService> _mockEntertainmentService;
    private readonly FFTHighFrequencyEffect _effect;

    public FFTHighFrequencyEffectTests()
    {
        _mockLogger = new Mock<ILogger<FFTHighFrequencyEffect>>();
        _mockEntertainmentService = new Mock<IEntertainmentService>();
        _effect = new FFTHighFrequencyEffect(_mockLogger.Object, _mockEntertainmentService.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new FFTHighFrequencyEffect(null!, _mockEntertainmentService.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullEntertainmentService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new FFTHighFrequencyEffect(_mockLogger.Object, null!));
        Assert.Equal("entertainmentService", exception.ParamName);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("FFTHighFrequencyEffect", _effect.Name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Assert
        Assert.Contains("DTLS", _effect.Description);
        Assert.Contains("Treble", _effect.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("high frequency", _effect.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void State_InitiallyUninitialized()
    {
        // Assert
        Assert.Equal(EffectState.Uninitialized, _effect.State);
    }

    [Fact]
    public async Task InitializeAsync_WithValidConfig_ReturnsTrue()
    {
        // Arrange
        var area = new EntertainmentArea
        {
            Id = Guid.NewGuid(),
            Name = "Test Area",
            LightIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
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
    public async Task StartAsync_FromInitializedState_StartsEffect()
    {
        // Arrange
        var config = new EffectConfig();
        await _effect.InitializeAsync(config);
        _mockEntertainmentService.Setup(e => e.IsStreaming).Returns(true);

        // Act
        await _effect.StartAsync();

        // Assert
        Assert.Equal(EffectState.Running, _effect.State);
    }

    [Fact]
    public void OnSpectralData_WithAudioReactive_ReactsToHighFrequency()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = true, AudioSensitivity = 0.5 };
        _effect.UpdateConfig(config);
        _mockEntertainmentService.Setup(e => e.IsStreaming).Returns(true);
        var spectralData = new SpectralDataEventArgs(0.1, 0.2, 0.9);

        // Act & Assert - should not throw
        var exception = Record.Exception(() => _effect.OnSpectralData(spectralData));
        Assert.Null(exception);
    }

    [Fact]
    public void OnBeatDetected_WithAudioReactive_FlashesLights()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = true };
        _effect.UpdateConfig(config);
        _mockEntertainmentService.Setup(e => e.IsStreaming).Returns(true);
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
