using LightJockey.Models;
using LightJockey.Services;
using LightJockey.Services.Effects;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services.Effects;

/// <summary>
/// Unit tests for RainbowCycleEffect
/// </summary>
public class RainbowCycleEffectTests : IDisposable
{
    private readonly Mock<ILogger<RainbowCycleEffect>> _mockLogger;
    private readonly Mock<IHueService> _mockHueService;
    private readonly RainbowCycleEffect _effect;

    public RainbowCycleEffectTests()
    {
        _mockLogger = new Mock<ILogger<RainbowCycleEffect>>();
        _mockHueService = new Mock<IHueService>();
        _effect = new RainbowCycleEffect(_mockLogger.Object, _mockHueService.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new RainbowCycleEffect(null!, _mockHueService.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullHueService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new RainbowCycleEffect(_mockLogger.Object, null!));
        Assert.Equal("hueService", exception.ParamName);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("RainbowCycleEffect", _effect.Name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Assert
        Assert.Contains("HTTPS", _effect.Description);
        Assert.Contains("rainbow", _effect.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void State_InitiallyUninitialized()
    {
        // Assert
        Assert.Equal(EffectState.Uninitialized, _effect.State);
    }

    [Fact]
    public async Task InitializeAsync_WithDisconnectedHueService_ReturnsFalse()
    {
        // Arrange
        _mockHueService.Setup(h => h.IsConnected).Returns(false);
        var config = new EffectConfig();

        // Act
        var result = await _effect.InitializeAsync(config);

        // Assert
        Assert.False(result);
        Assert.Equal(EffectState.Error, _effect.State);
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
        var config = new EffectConfig();

        // Act
        var result = await _effect.InitializeAsync(config);

        // Assert
        Assert.True(result);
        Assert.Equal(EffectState.Initialized, _effect.State);
    }

    [Fact]
    public async Task InitializeAsync_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _effect.InitializeAsync(null!));
    }

    [Fact]
    public async Task StartAsync_FromInitializedState_StartsEffect()
    {
        // Arrange
        _mockHueService.Setup(h => h.IsConnected).Returns(true);
        _mockHueService.Setup(h => h.GetLightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HueLight>());
        var config = new EffectConfig();
        await _effect.InitializeAsync(config);

        // Act
        await _effect.StartAsync();

        // Assert
        Assert.Equal(EffectState.Running, _effect.State);
    }

    [Fact]
    public void UpdateConfig_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _effect.UpdateConfig(null!));
    }

    [Fact]
    public void OnSpectralData_DoesNotThrow()
    {
        // Arrange
        var spectralData = new SpectralDataEventArgs(0.5, 0.5, 0.5);

        // Act & Assert
        var exception = Record.Exception(() => _effect.OnSpectralData(spectralData));
        Assert.Null(exception);
    }

    [Fact]
    public void OnBeatDetected_DoesNotThrow()
    {
        // Arrange
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
