using LightJockey.Models;
using LightJockey.Services;
using LightJockey.Services.Effects;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services.Effects;

/// <summary>
/// Unit tests for FastEntertainmentEffect
/// </summary>
public class FastEntertainmentEffectTests : IDisposable
{
    private readonly Mock<ILogger<FastEntertainmentEffect>> _mockLogger;
    private readonly Mock<IEntertainmentService> _mockEntertainmentService;
    private readonly FastEntertainmentEffect _effect;

    public FastEntertainmentEffectTests()
    {
        _mockLogger = new Mock<ILogger<FastEntertainmentEffect>>();
        _mockEntertainmentService = new Mock<IEntertainmentService>();
        _effect = new FastEntertainmentEffect(_mockLogger.Object, _mockEntertainmentService.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new FastEntertainmentEffect(null!, _mockEntertainmentService.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullEntertainmentService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => 
            new FastEntertainmentEffect(_mockLogger.Object, null!));
        Assert.Equal("entertainmentService", exception.ParamName);
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        // Assert
        Assert.Equal("FastEntertainmentEffect", _effect.Name);
    }

    [Fact]
    public void Description_ReturnsCorrectValue()
    {
        // Assert
        Assert.Contains("DTLS/UDP", _effect.Description);
        Assert.Contains("Entertainment V2", _effect.Description);
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
            LightIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
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
        var config = new EffectConfig();
        await _effect.InitializeAsync(config);
        _mockEntertainmentService.Setup(e => e.IsStreaming).Returns(true);

        // Act
        await _effect.StartAsync();

        // Assert
        Assert.Equal(EffectState.Running, _effect.State);
    }

    [Fact]
    public async Task StartAsync_StartsStreamingIfNotAlreadyStreaming()
    {
        // Arrange
        var config = new EffectConfig();
        var entertainmentConfig = new LightJockeyEntertainmentConfig();
        await _effect.InitializeAsync(config);
        _mockEntertainmentService.Setup(e => e.IsStreaming).Returns(false);
        _mockEntertainmentService.Setup(e => e.Configuration).Returns(entertainmentConfig);
        _mockEntertainmentService.Setup(e => e.StartStreamingAsync(
            It.IsAny<LightJockeyEntertainmentConfig>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _effect.StartAsync();

        // Assert
        _mockEntertainmentService.Verify(e => e.StartStreamingAsync(
            entertainmentConfig, 
            It.IsAny<CancellationToken>()), 
            Times.Once);
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
        var config = new EffectConfig();
        await _effect.InitializeAsync(config);
        _mockEntertainmentService.Setup(e => e.IsStreaming).Returns(true);
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
        var config = new EffectConfig();
        await _effect.InitializeAsync(config);
        _mockEntertainmentService.Setup(e => e.IsStreaming).Returns(true);
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
    public async Task OnSpectralData_WithAudioReactiveEnabled_UpdatesChannels()
    {
        // Arrange
        var area = new EntertainmentArea
        {
            Id = Guid.NewGuid(),
            Name = "Test Area",
            LightIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }
        };
        _mockEntertainmentService.Setup(e => e.ActiveArea).Returns(area);
        _mockEntertainmentService.Setup(e => e.IsStreaming).Returns(true);
        _mockEntertainmentService.Setup(e => e.UpdateChannel(
            It.IsAny<byte>(), 
            It.IsAny<HueColor>(), 
            It.IsAny<double>()));

        var config = new EffectConfig { AudioReactive = true };
        await _effect.InitializeAsync(config);
        await _effect.StartAsync();

        var spectralData = new SpectralDataEventArgs(0.5, 0.6, 0.7);

        // Act
        _effect.OnSpectralData(spectralData);

        // Assert
        _mockEntertainmentService.Verify(e => e.UpdateChannel(
            It.IsAny<byte>(), 
            It.IsAny<HueColor>(), 
            It.IsAny<double>()), 
            Times.AtLeast(3)); // Should update all 3 channels
    }

    [Fact]
    public void OnSpectralData_WithAudioReactiveDisabled_DoesNotUpdate()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = false };
        _effect.UpdateConfig(config);
        var spectralData = new SpectralDataEventArgs(0.5, 0.6, 0.7);

        // Act
        _effect.OnSpectralData(spectralData);

        // Assert
        _mockEntertainmentService.Verify(e => e.UpdateChannel(
            It.IsAny<byte>(), 
            It.IsAny<HueColor>(), 
            It.IsAny<double>()), 
            Times.Never);
    }

    [Fact]
    public void OnSpectralData_WhenNotStreaming_DoesNotUpdate()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = true };
        _effect.UpdateConfig(config);
        _mockEntertainmentService.Setup(e => e.IsStreaming).Returns(false);
        var spectralData = new SpectralDataEventArgs(0.5, 0.6, 0.7);

        // Act
        _effect.OnSpectralData(spectralData);

        // Assert
        _mockEntertainmentService.Verify(e => e.UpdateChannel(
            It.IsAny<byte>(), 
            It.IsAny<HueColor>(), 
            It.IsAny<double>()), 
            Times.Never);
    }

    [Fact]
    public async Task OnBeatDetected_WithAudioReactiveEnabled_FlashesChannels()
    {
        // Arrange
        var area = new EntertainmentArea
        {
            Id = Guid.NewGuid(),
            Name = "Test Area",
            LightIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
        };
        _mockEntertainmentService.Setup(e => e.ActiveArea).Returns(area);
        _mockEntertainmentService.Setup(e => e.IsStreaming).Returns(true);
        _mockEntertainmentService.Setup(e => e.UpdateChannel(
            It.IsAny<byte>(), 
            It.IsAny<HueColor>(), 
            It.IsAny<double>()));

        var config = new EffectConfig { AudioReactive = true };
        await _effect.InitializeAsync(config);
        await _effect.StartAsync();

        var beatData = new BeatDetectedEventArgs(0.8, 120, 0.9);

        // Act
        _effect.OnBeatDetected(beatData);

        // Assert
        _mockEntertainmentService.Verify(e => e.UpdateChannel(
            It.IsAny<byte>(), 
            It.IsAny<HueColor>(), 
            It.IsAny<double>()), 
            Times.AtLeast(2)); // Should flash all channels
    }

    [Fact]
    public void OnBeatDetected_WithAudioReactiveDisabled_DoesNotFlash()
    {
        // Arrange
        var config = new EffectConfig { AudioReactive = false };
        _effect.UpdateConfig(config);
        var beatData = new BeatDetectedEventArgs(0.8, 120, 0.9);

        // Act
        _effect.OnBeatDetected(beatData);

        // Assert
        _mockEntertainmentService.Verify(e => e.UpdateChannel(
            It.IsAny<byte>(), 
            It.IsAny<HueColor>(), 
            It.IsAny<double>()), 
            Times.Never);
    }

    [Fact]
    public void StateChanged_EventRaisedOnStateChange()
    {
        // Arrange
        EffectState? newState = null;
        _effect.StateChanged += (sender, state) => newState = state;
        var config = new EffectConfig();

        // Act
        _effect.InitializeAsync(config).Wait();

        // Assert
        Assert.Equal(EffectState.Initialized, newState);
    }

    public void Dispose()
    {
        _effect?.Dispose();
    }
}
