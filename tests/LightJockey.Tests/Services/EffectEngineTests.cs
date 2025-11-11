using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.Services;

/// <summary>
/// Unit tests for EffectEngine
/// </summary>
public class EffectEngineTests : IDisposable
{
    private readonly Mock<ILogger<EffectEngine>> _mockLogger;
    private readonly Mock<IAudioService> _mockAudioService;
    private readonly Mock<ISpectralAnalyzer> _mockSpectralAnalyzer;
    private readonly Mock<IBeatDetector> _mockBeatDetector;
    private readonly EffectEngine _effectEngine;
    private readonly Mock<IEffectPlugin> _mockPlugin;

    public EffectEngineTests()
    {
        _mockLogger = new Mock<ILogger<EffectEngine>>();
        _mockAudioService = new Mock<IAudioService>();
        _mockSpectralAnalyzer = new Mock<ISpectralAnalyzer>();
        _mockBeatDetector = new Mock<IBeatDetector>();
        
        _effectEngine = new EffectEngine(
            _mockLogger.Object,
            _mockAudioService.Object,
            _mockSpectralAnalyzer.Object,
            _mockBeatDetector.Object);

        _mockPlugin = new Mock<IEffectPlugin>();
        _mockPlugin.Setup(p => p.Name).Returns("TestPlugin");
        _mockPlugin.Setup(p => p.Description).Returns("Test plugin description");
        _mockPlugin.Setup(p => p.State).Returns(EffectState.Initialized);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EffectEngine(
            null!,
            _mockAudioService.Object,
            _mockSpectralAnalyzer.Object,
            _mockBeatDetector.Object));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullAudioService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EffectEngine(
            _mockLogger.Object,
            null!,
            _mockSpectralAnalyzer.Object,
            _mockBeatDetector.Object));
        Assert.Equal("audioService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullSpectralAnalyzer_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EffectEngine(
            _mockLogger.Object,
            _mockAudioService.Object,
            null!,
            _mockBeatDetector.Object));
        Assert.Equal("spectralAnalyzer", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullBeatDetector_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new EffectEngine(
            _mockLogger.Object,
            _mockAudioService.Object,
            _mockSpectralAnalyzer.Object,
            null!));
        Assert.Equal("beatDetector", exception.ParamName);
    }

    [Fact]
    public void RegisterPlugin_WithValidPlugin_AddsPluginToRegistry()
    {
        // Act
        _effectEngine.RegisterPlugin(_mockPlugin.Object);

        // Assert
        var availableEffects = _effectEngine.GetAvailableEffects();
        Assert.Contains("TestPlugin", availableEffects);
    }

    [Fact]
    public void RegisterPlugin_WithNullPlugin_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _effectEngine.RegisterPlugin(null!));
    }

    [Fact]
    public void RegisterPlugin_WithDuplicateName_DoesNotThrow()
    {
        // Arrange
        _effectEngine.RegisterPlugin(_mockPlugin.Object);

        // Act
        var exception = Record.Exception(() => _effectEngine.RegisterPlugin(_mockPlugin.Object));

        // Assert - Should log warning but not throw
        Assert.Null(exception);
    }

    [Fact]
    public void UnregisterPlugin_WithExistingPlugin_RemovesPlugin()
    {
        // Arrange
        _effectEngine.RegisterPlugin(_mockPlugin.Object);

        // Act
        _effectEngine.UnregisterPlugin("TestPlugin");

        // Assert
        var availableEffects = _effectEngine.GetAvailableEffects();
        Assert.DoesNotContain("TestPlugin", availableEffects);
    }

    [Fact]
    public void UnregisterPlugin_WithNullOrEmptyName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _effectEngine.UnregisterPlugin(null!));
        Assert.Throws<ArgumentException>(() => _effectEngine.UnregisterPlugin(string.Empty));
    }

    [Fact]
    public void GetAvailableEffects_WithNoPlugins_ReturnsEmptyList()
    {
        // Act
        var effects = _effectEngine.GetAvailableEffects();

        // Assert
        Assert.Empty(effects);
    }

    [Fact]
    public void GetAvailableEffects_WithRegisteredPlugins_ReturnsAllPluginNames()
    {
        // Arrange
        var plugin1 = new Mock<IEffectPlugin>();
        plugin1.Setup(p => p.Name).Returns("Plugin1");
        var plugin2 = new Mock<IEffectPlugin>();
        plugin2.Setup(p => p.Name).Returns("Plugin2");

        _effectEngine.RegisterPlugin(plugin1.Object);
        _effectEngine.RegisterPlugin(plugin2.Object);

        // Act
        var effects = _effectEngine.GetAvailableEffects();

        // Assert
        Assert.Equal(2, effects.Count);
        Assert.Contains("Plugin1", effects);
        Assert.Contains("Plugin2", effects);
    }

    [Fact]
    public void GetPlugin_WithExistingName_ReturnsPlugin()
    {
        // Arrange
        _effectEngine.RegisterPlugin(_mockPlugin.Object);

        // Act
        var plugin = _effectEngine.GetPlugin("TestPlugin");

        // Assert
        Assert.NotNull(plugin);
        Assert.Equal("TestPlugin", plugin.Name);
    }

    [Fact]
    public void GetPlugin_WithNonExistingName_ReturnsNull()
    {
        // Act
        var plugin = _effectEngine.GetPlugin("NonExistent");

        // Assert
        Assert.Null(plugin);
    }

    [Fact]
    public void GetPlugin_WithNullOrEmptyName_ReturnsNull()
    {
        // Act
        var plugin1 = _effectEngine.GetPlugin(null!);
        var plugin2 = _effectEngine.GetPlugin(string.Empty);

        // Assert
        Assert.Null(plugin1);
        Assert.Null(plugin2);
    }

    [Fact]
    public async Task SetActiveEffectAsync_WithValidPlugin_ActivatesEffect()
    {
        // Arrange
        var config = new EffectConfig { Intensity = 0.8 };
        _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
        _mockPlugin.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _effectEngine.RegisterPlugin(_mockPlugin.Object);

        // Act
        var result = await _effectEngine.SetActiveEffectAsync("TestPlugin", config);

        // Assert
        Assert.True(result);
        Assert.Equal("TestPlugin", _effectEngine.ActiveEffectName);
        Assert.NotNull(_effectEngine.ActiveEffect);
        _mockPlugin.Verify(p => p.InitializeAsync(config), Times.Once);
        _mockPlugin.Verify(p => p.StartAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetActiveEffectAsync_WithNonExistingPlugin_ReturnsFalse()
    {
        // Arrange
        var config = new EffectConfig();

        // Act
        var result = await _effectEngine.SetActiveEffectAsync("NonExistent", config);

        // Assert
        Assert.False(result);
        Assert.Null(_effectEngine.ActiveEffectName);
    }

    [Fact]
    public async Task SetActiveEffectAsync_WithNullOrEmptyName_ThrowsArgumentException()
    {
        // Arrange
        var config = new EffectConfig();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _effectEngine.SetActiveEffectAsync(null!, config));
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _effectEngine.SetActiveEffectAsync(string.Empty, config));
    }

    [Fact]
    public async Task SetActiveEffectAsync_WithNullConfig_ThrowsArgumentNullException()
    {
        // Arrange
        _effectEngine.RegisterPlugin(_mockPlugin.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => 
            _effectEngine.SetActiveEffectAsync("TestPlugin", null!));
    }

    [Fact]
    public async Task SetActiveEffectAsync_WhenInitializationFails_ReturnsFalse()
    {
        // Arrange
        var config = new EffectConfig();
        _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(false);
        _effectEngine.RegisterPlugin(_mockPlugin.Object);

        // Act
        var result = await _effectEngine.SetActiveEffectAsync("TestPlugin", config);

        // Assert
        Assert.False(result);
        Assert.Null(_effectEngine.ActiveEffectName);
    }

    [Fact]
    public async Task SetActiveEffectAsync_StopsPreviousActiveEffect()
    {
        // Arrange
        var plugin1 = new Mock<IEffectPlugin>();
        plugin1.Setup(p => p.Name).Returns("Plugin1");
        plugin1.Setup(p => p.InitializeAsync(It.IsAny<EffectConfig>())).ReturnsAsync(true);
        plugin1.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        plugin1.Setup(p => p.StopAsync()).Returns(Task.CompletedTask);

        var plugin2 = new Mock<IEffectPlugin>();
        plugin2.Setup(p => p.Name).Returns("Plugin2");
        plugin2.Setup(p => p.InitializeAsync(It.IsAny<EffectConfig>())).ReturnsAsync(true);
        plugin2.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        _effectEngine.RegisterPlugin(plugin1.Object);
        _effectEngine.RegisterPlugin(plugin2.Object);

        var config = new EffectConfig();
        await _effectEngine.SetActiveEffectAsync("Plugin1", config);

        // Act
        await _effectEngine.SetActiveEffectAsync("Plugin2", config);

        // Assert
        plugin1.Verify(p => p.StopAsync(), Times.Once);
        Assert.Equal("Plugin2", _effectEngine.ActiveEffectName);
    }

    [Fact]
    public async Task StopActiveEffectAsync_WithActiveEffect_StopsEffect()
    {
        // Arrange
        var config = new EffectConfig();
        _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
        _mockPlugin.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockPlugin.Setup(p => p.StopAsync()).Returns(Task.CompletedTask);
        _effectEngine.RegisterPlugin(_mockPlugin.Object);
        await _effectEngine.SetActiveEffectAsync("TestPlugin", config);

        // Act
        await _effectEngine.StopActiveEffectAsync();

        // Assert
        _mockPlugin.Verify(p => p.StopAsync(), Times.Once);
        Assert.Null(_effectEngine.ActiveEffectName);
        Assert.Null(_effectEngine.ActiveEffect);
    }

    [Fact]
    public async Task StopActiveEffectAsync_WithNoActiveEffect_DoesNothing()
    {
        // Act
        var exception = await Record.ExceptionAsync(() => _effectEngine.StopActiveEffectAsync());

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void UpdateActiveEffectConfig_WithActiveEffect_UpdatesConfig()
    {
        // Arrange
        var config1 = new EffectConfig { Intensity = 0.5 };
        var config2 = new EffectConfig { Intensity = 0.8 };
        _mockPlugin.Setup(p => p.InitializeAsync(config1)).ReturnsAsync(true);
        _mockPlugin.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _effectEngine.RegisterPlugin(_mockPlugin.Object);
        _effectEngine.SetActiveEffectAsync("TestPlugin", config1).Wait();

        // Act
        _effectEngine.UpdateActiveEffectConfig(config2);

        // Assert
        _mockPlugin.Verify(p => p.UpdateConfig(config2), Times.Once);
    }

    [Fact]
    public void UpdateActiveEffectConfig_WithNoActiveEffect_DoesNotThrow()
    {
        // Arrange
        var config = new EffectConfig();

        // Act
        var exception = Record.Exception(() => _effectEngine.UpdateActiveEffectConfig(config));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void UpdateActiveEffectConfig_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _effectEngine.UpdateActiveEffectConfig(null!));
    }

    [Fact]
    public void IsEffectRunning_WithNoActiveEffect_ReturnsFalse()
    {
        // Act & Assert
        Assert.False(_effectEngine.IsEffectRunning);
    }

    [Fact]
    public async Task IsEffectRunning_WithRunningEffect_ReturnsTrue()
    {
        // Arrange
        var config = new EffectConfig();
        _mockPlugin.Setup(p => p.State).Returns(EffectState.Running);
        _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
        _mockPlugin.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _effectEngine.RegisterPlugin(_mockPlugin.Object);
        await _effectEngine.SetActiveEffectAsync("TestPlugin", config);

        // Act & Assert
        Assert.True(_effectEngine.IsEffectRunning);
    }

    [Fact]
    public async Task ActiveEffectChanged_RaisedWhenEffectActivated()
    {
        // Arrange
        var config = new EffectConfig();
        _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
        _mockPlugin.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _effectEngine.RegisterPlugin(_mockPlugin.Object);

        string? activatedEffect = null;
        _effectEngine.ActiveEffectChanged += (sender, effectName) => activatedEffect = effectName;

        // Act
        await _effectEngine.SetActiveEffectAsync("TestPlugin", config);

        // Assert
        Assert.Equal("TestPlugin", activatedEffect);
    }

    [Fact]
    public async Task ActiveEffectChanged_RaisedWhenEffectStopped()
    {
        // Arrange
        var config = new EffectConfig();
        _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
        _mockPlugin.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockPlugin.Setup(p => p.StopAsync()).Returns(Task.CompletedTask);
        _effectEngine.RegisterPlugin(_mockPlugin.Object);
        await _effectEngine.SetActiveEffectAsync("TestPlugin", config);

        string? stoppedEffect = "NotNull";
        _effectEngine.ActiveEffectChanged += (sender, effectName) => stoppedEffect = effectName;

        // Act
        await _effectEngine.StopActiveEffectAsync();

        // Assert
        Assert.Null(stoppedEffect);
    }

    [Fact]
    public void SpectralDataEvent_ForwardsToActiveEffect()
    {
        // Arrange
        var config = new EffectConfig();
        _mockPlugin.Setup(p => p.State).Returns(EffectState.Running);
        _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
        _mockPlugin.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _effectEngine.RegisterPlugin(_mockPlugin.Object);
        _effectEngine.SetActiveEffectAsync("TestPlugin", config).Wait();

        var spectralData = new SpectralDataEventArgs(0.5, 0.6, 0.7);

        // Act
        _mockSpectralAnalyzer.Raise(sa => sa.SpectralDataAvailable += null, _mockSpectralAnalyzer.Object, spectralData);

        // Assert
        _mockPlugin.Verify(p => p.OnSpectralData(spectralData), Times.Once);
    }

    [Fact]
    public void BeatDetectedEvent_ForwardsToActiveEffect()
    {
        // Arrange
        var config = new EffectConfig();
        _mockPlugin.Setup(p => p.State).Returns(EffectState.Running);
        _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
        _mockPlugin.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _effectEngine.RegisterPlugin(_mockPlugin.Object);
        _effectEngine.SetActiveEffectAsync("TestPlugin", config).Wait();

        var beatData = new BeatDetectedEventArgs(0.8, 120, 0.9);

        // Act
        _mockBeatDetector.Raise(bd => bd.BeatDetected += null, _mockBeatDetector.Object, beatData);

        // Assert
        _mockPlugin.Verify(p => p.OnBeatDetected(beatData), Times.Once);
    }

    [Fact]
    public void Dispose_StopsActiveEffectAndDisposesPlugins()
    {
        // Arrange
        var config = new EffectConfig();
        _mockPlugin.Setup(p => p.InitializeAsync(config)).ReturnsAsync(true);
        _mockPlugin.Setup(p => p.StartAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockPlugin.Setup(p => p.StopAsync()).Returns(Task.CompletedTask);
        _effectEngine.RegisterPlugin(_mockPlugin.Object);
        _effectEngine.SetActiveEffectAsync("TestPlugin", config).Wait();

        // Act
        _effectEngine.Dispose();

        // Assert
        _mockPlugin.Verify(p => p.StopAsync(), Times.Once);
        _mockPlugin.Verify(p => p.Dispose(), Times.Once);
    }

    public void Dispose()
    {
        _effectEngine?.Dispose();
    }
}
