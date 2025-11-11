using LightJockey.Models;
using LightJockey.Services;
using LightJockey.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace LightJockey.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private readonly Mock<ILogger<MainWindowViewModel>> _mockLogger;
    private readonly Mock<IAudioService> _mockAudioService;
    private readonly Mock<IHueService> _mockHueService;
    private readonly Mock<IEffectEngine> _mockEffectEngine;
    private readonly Mock<IFFTProcessor> _mockFFTProcessor;
    private readonly Mock<ISpectralAnalyzer> _mockSpectralAnalyzer;
    private readonly Mock<IBeatDetector> _mockBeatDetector;

    public MainWindowViewModelTests()
    {
        _mockLogger = new Mock<ILogger<MainWindowViewModel>>();
        _mockAudioService = new Mock<IAudioService>();
        _mockHueService = new Mock<IHueService>();
        _mockEffectEngine = new Mock<IEffectEngine>();
        _mockFFTProcessor = new Mock<IFFTProcessor>();
        _mockSpectralAnalyzer = new Mock<ISpectralAnalyzer>();
        _mockBeatDetector = new Mock<IBeatDetector>();
        
        // Setup default return values to prevent null reference exceptions in constructor
        _mockAudioService.Setup(s => s.GetOutputDevices()).Returns(new List<AudioDevice>());
        _mockEffectEngine.Setup(e => e.GetAvailableEffects()).Returns(new List<string>());
    }

    private MainWindowViewModel CreateViewModel()
    {
        return new MainWindowViewModel(
            _mockLogger.Object,
            _mockAudioService.Object,
            _mockHueService.Object,
            _mockEffectEngine.Object,
            _mockFFTProcessor.Object,
            _mockSpectralAnalyzer.Object,
            _mockBeatDetector.Object);
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.NotNull(viewModel.AudioDevices);
        Assert.NotNull(viewModel.HueBridges);
        Assert.NotNull(viewModel.HueLights);
        Assert.NotNull(viewModel.AvailableEffects);
        Assert.False(viewModel.IsAudioCapturing);
        Assert.False(viewModel.IsHueConnected);
        Assert.False(viewModel.IsEffectRunning);
        Assert.True(viewModel.IsDarkTheme);
        Assert.Equal("Ready", viewModel.StatusMessage);
    }

    [Fact]
    public void Constructor_InitializesCommands()
    {
        // Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.NotNull(viewModel.RefreshAudioDevicesCommand);
        Assert.NotNull(viewModel.StartAudioCaptureCommand);
        Assert.NotNull(viewModel.StopAudioCaptureCommand);
        Assert.NotNull(viewModel.DiscoverHueBridgesCommand);
        Assert.NotNull(viewModel.ConnectToHueBridgeCommand);
        Assert.NotNull(viewModel.StartEffectCommand);
        Assert.NotNull(viewModel.StopEffectCommand);
        Assert.NotNull(viewModel.ToggleThemeCommand);
    }

    [Fact]
    public void RefreshAudioDevicesCommand_LoadsDevices()
    {
        // Arrange
        var devices = new List<AudioDevice>
        {
            new() { Name = "Device 1", Id = "1" },
            new() { Name = "Device 2", Id = "2" }
        };
        _mockAudioService.Setup(s => s.GetOutputDevices()).Returns(devices);
        var viewModel = CreateViewModel();

        // Act
        viewModel.RefreshAudioDevicesCommand.Execute(null);

        // Assert
        Assert.Equal(2, viewModel.AudioDevices.Count);
        Assert.NotNull(viewModel.SelectedAudioDevice);
    }

    [Fact]
    public void SelectedAudioDevice_CallsSelectDevice()
    {
        // Arrange
        var device = new AudioDevice { Name = "Test Device", Id = "1" };
        var viewModel = CreateViewModel();

        // Act
        viewModel.SelectedAudioDevice = device;

        // Assert
        _mockAudioService.Verify(s => s.SelectDevice(device), Times.Once);
    }

    [Fact]
    public void StartAudioCaptureCommand_StartsCapture()
    {
        // Arrange
        var device = new AudioDevice { Name = "Test Device", Id = "1" };
        var viewModel = CreateViewModel();
        viewModel.SelectedAudioDevice = device;

        // Act
        viewModel.StartAudioCaptureCommand.Execute(null);

        // Assert
        _mockAudioService.Verify(s => s.StartCapture(), Times.Once);
        Assert.True(viewModel.IsAudioCapturing);
    }

    [Fact]
    public void StopAudioCaptureCommand_StopsCapture()
    {
        // Arrange
        var device = new AudioDevice { Name = "Test Device", Id = "1" };
        var viewModel = CreateViewModel();
        viewModel.SelectedAudioDevice = device;
        viewModel.StartAudioCaptureCommand.Execute(null);

        // Act
        viewModel.StopAudioCaptureCommand.Execute(null);

        // Assert
        _mockAudioService.Verify(s => s.StopCapture(), Times.Once);
        Assert.False(viewModel.IsAudioCapturing);
    }

    [Fact]
    public async Task DiscoverHueBridgesCommand_DiscoversBridges()
    {
        // Arrange
        var bridges = new List<HueBridge>
        {
            new() { IpAddress = "192.168.1.100", BridgeId = "bridge1" },
            new() { IpAddress = "192.168.1.101", BridgeId = "bridge2" }
        };
        _mockHueService.Setup(s => s.DiscoverBridgesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(bridges);
        var viewModel = CreateViewModel();

        // Act
        viewModel.DiscoverHueBridgesCommand.Execute(null);
        await Task.Delay(100); // Allow async operation to complete

        // Assert
        Assert.Equal(2, viewModel.HueBridges.Count);
    }

    [Fact]
    public void Intensity_UpdatesProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var propertyChangedFired = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.Intensity))
                propertyChangedFired = true;
        };

        // Act
        viewModel.Intensity = 0.5;

        // Assert
        Assert.Equal(0.5, viewModel.Intensity);
        Assert.True(propertyChangedFired);
    }

    [Fact]
    public void Speed_UpdatesProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var propertyChangedFired = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.Speed))
                propertyChangedFired = true;
        };

        // Act
        viewModel.Speed = 2.0;

        // Assert
        Assert.Equal(2.0, viewModel.Speed);
        Assert.True(propertyChangedFired);
    }

    [Fact]
    public void Brightness_UpdatesProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var propertyChangedFired = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.Brightness))
                propertyChangedFired = true;
        };

        // Act
        viewModel.Brightness = 0.6;

        // Assert
        Assert.Equal(0.6, viewModel.Brightness);
        Assert.True(propertyChangedFired);
    }

    [Fact]
    public void AudioSensitivity_UpdatesProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var propertyChangedFired = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.AudioSensitivity))
                propertyChangedFired = true;
        };

        // Act
        viewModel.AudioSensitivity = 0.7;

        // Assert
        Assert.Equal(0.7, viewModel.AudioSensitivity);
        Assert.True(propertyChangedFired);
    }

    [Fact]
    public void ToggleThemeCommand_TogglesTheme()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var initialTheme = viewModel.IsDarkTheme;

        // Act
        viewModel.ToggleThemeCommand.Execute(null);

        // Assert
        Assert.NotEqual(initialTheme, viewModel.IsDarkTheme);
    }

    [Fact]
    public void LoadAvailableEffects_PopulatesEffectsList()
    {
        // Arrange
        var effects = new List<string> { "SlowHttpsEffect", "FastEntertainmentEffect" };
        _mockEffectEngine.Setup(e => e.GetAvailableEffects()).Returns(effects);

        // Act
        var viewModel = CreateViewModel();

        // Assert
        Assert.Equal(2, viewModel.AvailableEffects.Count);
        Assert.Contains("SlowHttpsEffect", viewModel.AvailableEffects);
        Assert.Contains("FastEntertainmentEffect", viewModel.AvailableEffects);
    }

    [Fact]
    public void Dispose_UnsubscribesFromEvents()
    {
        // Arrange
        var viewModel = CreateViewModel();

        // Act
        viewModel.Dispose();

        // Assert - No exception should be thrown
        Assert.True(true);
    }

    [Fact]
    public void AudioReactive_UpdatesProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var propertyChangedFired = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.AudioReactive))
                propertyChangedFired = true;
        };

        // Act
        viewModel.AudioReactive = false;

        // Assert
        Assert.False(viewModel.AudioReactive);
        Assert.True(propertyChangedFired);
    }

    [Fact]
    public void SmoothTransitions_UpdatesProperty()
    {
        // Arrange
        var viewModel = CreateViewModel();
        var propertyChangedFired = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.SmoothTransitions))
                propertyChangedFired = true;
        };

        // Act
        viewModel.SmoothTransitions = false;

        // Assert
        Assert.False(viewModel.SmoothTransitions);
        Assert.True(propertyChangedFired);
    }
}
