using LightJockey.Models;
using LightJockey.Services;
using LightJockey.Services.Effects;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services.Effects;

/// <summary>
/// Unit tests for FFTLowFrequencyEffect
/// </summary>
public class FFTLowFrequencyEffectTests : IDisposable
{
    private readonly Mock<ILogger<FFTLowFrequencyEffect>> _mockLogger;
    private readonly Mock<IHueService> _mockHueService;
    private readonly FFTLowFrequencyEffect _effect;

    public FFTLowFrequencyEffectTests()
    {
        _mockLogger = new Mock<ILogger<FFTLowFrequencyEffect>>();
        _mockHueService = new Mock<IHueService>();
        _effect = new FFTLowFrequencyEffect(_mockLogger.Object, _mockHueService.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new FFTLowFrequencyEffect(null!, _mockHueService.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullHueService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new FFTLowFrequencyEffect(_mockLogger.Object, null!));
        Assert.Equal("hueService", exception.ParamName);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("FFTLowFrequencyEffect", _effect.Name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Assert
        Assert.Contains("HTTPS", _effect.Description);
        Assert.Contains("Bass", _effect.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("low frequency", _effect.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void State_InitiallyUninitialized()
    {
        // Assert
        Assert.Equal(EffectState.Uninitialized, _effect.State);
    }

    [Fact]
    public async Task InitializeAsync_WithConnectedHueService_ReturnsTrue()
    {
        // Arrange
        _mockHueService.Setup(h => h.IsConnected).Returns(true);
        _mockHueService.Setup(h => h.GetLightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HueLight>
            {
                new() { Id = "1", Name = "Light 1", IsOn = true }
            });
        var config = new EffectConfig { AudioReactive = true };

        // Act
        var result = await _effect.InitializeAsync(config);

        // Assert
        Assert.True(result);
        Assert.Equal(EffectState.Initialized, _effect.State);
    }

    [Fact]
    public void OnSpectralData_WithAudioReactive_ReactsToLowFrequency()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = true, AudioSensitivity = 0.5 };
        _effect.UpdateConfig(config);
        var spectralData = new SpectralDataEventArgs(0.8, 0.2, 0.1);

        // Act & Assert - should not throw
        var exception = Record.Exception(() => _effect.OnSpectralData(spectralData));
        Assert.Null(exception);
    }

    [Fact]
    public void OnBeatDetected_WithAudioReactive_DoesNotThrow()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = true };
        _effect.UpdateConfig(config);
        var beatData = new BeatDetectedEventArgs(0.8, 120.0, 0.9);

        // Act & Assert
        var exception = Record.Exception(() => _effect.OnBeatDetected(beatData));
        Assert.Null(exception);
    }

    public void Dispose()
    {
        _effect?.Dispose();
    }
}
