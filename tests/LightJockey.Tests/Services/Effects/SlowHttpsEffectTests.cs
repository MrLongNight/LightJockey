using LightJockey.Models;
using LightJockey.Services;
using LightJockey.Services.Effects;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services.Effects;

/// <summary>
/// Unit tests for SlowHttpsEffect
/// </summary>
public class SlowHttpsEffectTests : IDisposable
{
    private readonly Mock<ILogger<SlowHttpsEffect>> _mockLogger;
    private readonly Mock<IHueService> _mockHueService;
    private readonly SlowHttpsEffect _effect;

    public SlowHttpsEffectTests()
    {
        _mockLogger = new Mock<ILogger<SlowHttpsEffect>>();
        _mockHueService = new Mock<IHueService>();
        _effect = new SlowHttpsEffect(_mockLogger.Object, _mockHueService.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new SlowHttpsEffect(null!, _mockHueService.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullHueService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new SlowHttpsEffect(_mockLogger.Object, null!));
        Assert.Equal("hueService", exception.ParamName);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("SlowHttpsEffect", _effect.Name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Assert
        Assert.Contains("HTTPS", _effect.Description);
        Assert.Contains("audio-reactive", _effect.Description);
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
    public async Task StartAsync_FromUninitializedState_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _effect.StartAsync());
    }

    [Fact]
    public async Task StopAsync_FromRunningState_StopsEffect()
    {
        // Arrange
        _mockHueService.Setup(h => h.IsConnected).Returns(true);
        _mockHueService.Setup(h => h.GetLightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HueLight>());
        var config = new EffectConfig();
        await _effect.InitializeAsync(config);
        await _effect.StartAsync();

        // Act
        await _effect.StopAsync();

        // Assert
        Assert.Equal(EffectState.Stopped, _effect.State);
    }

    [Fact]
    public async Task StopAsync_FromStoppedState_DoesNothing()
    {
        // Arrange
        _mockHueService.Setup(h => h.IsConnected).Returns(true);
        _mockHueService.Setup(h => h.GetLightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HueLight>());
        var config = new EffectConfig();
        await _effect.InitializeAsync(config);
        await _effect.StartAsync();
        await _effect.StopAsync();

        // Act
        var exception = await Record.ExceptionAsync(() => _effect.StopAsync());

        // Assert
        Assert.Null(exception);
        Assert.Equal(EffectState.Stopped, _effect.State);
    }

    [Fact]
    public void UpdateConfig_WithValidConfig_UpdatesConfiguration()
    {
        // Arrange
        var config = new EffectConfig { Intensity = 0.9 };

        // Act
        var exception = Record.Exception(() => _effect.UpdateConfig(config));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void UpdateConfig_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _effect.UpdateConfig(null!));
    }

    [Fact]
    public void OnSpectralData_WithAudioReactiveEnabled_UpdatesInternalState()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = true };
        _effect.UpdateConfig(config);
        var spectralData = new SpectralDataEventArgs(0.5, 0.6, 0.7);

        // Act
        var exception = Record.Exception(() => _effect.OnSpectralData(spectralData));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void OnSpectralData_WithAudioReactiveDisabled_DoesNothing()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = false };
        _effect.UpdateConfig(config);
        var spectralData = new SpectralDataEventArgs(0.5, 0.6, 0.7);

        // Act
        var exception = Record.Exception(() => _effect.OnSpectralData(spectralData));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void OnBeatDetected_WithAudioReactiveEnabled_UpdatesInternalState()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = true };
        _effect.UpdateConfig(config);
        var beatData = new BeatDetectedEventArgs(0.8, 120, 0.9);

        // Act
        var exception = Record.Exception(() => _effect.OnBeatDetected(beatData));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void OnBeatDetected_WithAudioReactiveDisabled_DoesNothing()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = false };
        _effect.UpdateConfig(config);
        var beatData = new BeatDetectedEventArgs(0.8, 120, 0.9);

        // Act
        var exception = Record.Exception(() => _effect.OnBeatDetected(beatData));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void StateChanged_EventRaisedOnStateChange()
    {
        // Arrange
        EffectState? newState = null;
        _effect.StateChanged += (sender, state) => newState = state;
        _mockHueService.Setup(h => h.IsConnected).Returns(true);
        _mockHueService.Setup(h => h.GetLightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HueLight>());
        var config = new EffectConfig();

        // Act
        _effect.InitializeAsync(config).Wait();

        // Assert
        Assert.Equal(EffectState.Initialized, newState);
    }

    [Fact]
    public async Task Effect_UpdatesLights_WhenRunning()
    {
        // Arrange
        var light = new HueLight { Id = "1", Name = "Light 1", IsOn = true };
        _mockHueService.Setup(h => h.IsConnected).Returns(true);
        _mockHueService.Setup(h => h.GetLightsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<HueLight> { light });
        _mockHueService.Setup(h => h.SetLightColorAsync(It.IsAny<string>(), It.IsAny<HueColor>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockHueService.Setup(h => h.SetLightBrightnessAsync(It.IsAny<string>(), It.IsAny<byte>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var config = new EffectConfig { AudioReactive = true };
        await _effect.InitializeAsync(config);
        await _effect.StartAsync();

        // Trigger audio events
        var spectralData = new SpectralDataEventArgs(0.5, 0.6, 0.7);
        _effect.OnSpectralData(spectralData);

        // Wait a bit for the effect loop to run
        await Task.Delay(300);

        // Act & Assert - Stop the effect
        await _effect.StopAsync();

        // Verify lights were updated
        _mockHueService.Verify(h => h.SetLightColorAsync(
            It.IsAny<string>(), 
            It.IsAny<HueColor>(), 
            It.IsAny<CancellationToken>()), 
            Times.AtLeastOnce);
    }

    public void Dispose()
    {
        _effect?.Dispose();
    }
}
