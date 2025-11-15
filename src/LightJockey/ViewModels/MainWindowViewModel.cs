using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using LightJockey.Models;
using LightJockey.Services;
using LightJockey.Utilities;
using LightJockey.Views;
using Microsoft.Extensions.Logging;

namespace LightJockey.ViewModels;

/// <summary>
/// ViewModel for the main window
/// </summary>
public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IAudioService _audioService;
    private readonly IHueService _hueService;
    private readonly IEffectEngine _effectEngine;
    private readonly IFFTProcessor _fftProcessor;
    private readonly IConfigurationService _configurationService;
    private bool _disposed;

    // Audio devices
    private ObservableCollection<AudioDevice> _audioDevices = new();
    private AudioDevice? _selectedAudioDevice;
    private bool _isAudioCapturing;

    // Hue devices
    private ObservableCollection<HueBridge> _hueBridges = new();
    private HueBridge? _selectedHueBridge;
    private ObservableCollection<HueLight> _hueLights = new();
    private bool _isHueConnected;

    // Effects
    private ObservableCollection<string> _availableEffects = new();
    private string? _selectedEffect;
    private bool _isEffectRunning;

    // Effect parameters
    private double _intensity = 0.8;
    private double _speed = 1.0;
    private double _brightness = 0.8;
    private double _audioSensitivity = 0.5;
    private bool _audioReactive = true;
    private bool _smoothTransitions = true;
    private double _hueVariation = 0.5;
    private double _saturation = 0.8;
    private double _colorTemperature = 0.5;

    // Visualizer data
    private float[] _spectralData = Array.Empty<float>();
    private double _currentBpm;
    private bool _isBeatDetected;

    // Theme
    private bool _isDarkTheme = true;

    // Status messages
    private string _statusMessage = "Ready";

    public MetricsViewModel MetricsViewModel { get; }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IAudioService audioService,
        IHueService hueService,
        IEffectEngine effectEngine,
        IFFTProcessor fftProcessor,
        ISpectralAnalyzer spectralAnalyzer,
        IBeatDetector beatDetector,
        MetricsViewModel metricsViewModel,
        IConfigurationService configurationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _hueService = hueService ?? throw new ArgumentNullException(nameof(hueService));
        _effectEngine = effectEngine ?? throw new ArgumentNullException(nameof(effectEngine));
        _fftProcessor = fftProcessor ?? throw new ArgumentNullException(nameof(fftProcessor));
        _configurationService = configurationService;
        MetricsViewModel = metricsViewModel;

        // Subscribe to events
        _audioService.AudioDataAvailable += OnAudioDataAvailable;
        spectralAnalyzer.SpectralDataAvailable += OnSpectralDataAvailable;
        beatDetector.BeatDetected += OnBeatDetected;
        _effectEngine.ActiveEffectChanged += OnActiveEffectChanged;
        _effectEngine.EffectError += OnEffectError;

        // Initialize commands
        RefreshAudioDevicesCommand = new RelayCommand(_ => RefreshAudioDevices());
        StartAudioCaptureCommand = new RelayCommand(_ => StartAudioCapture(), _ => CanStartAudioCapture());
        StopAudioCaptureCommand = new RelayCommand(_ => StopAudioCapture(), _ => CanStopAudioCapture());
        
        DiscoverHueBridgesCommand = new RelayCommand(async _ => await DiscoverHueBridgesAsync());
        ConnectToHueBridgeCommand = new RelayCommand(async _ => await ConnectToHueBridgeAsync(), _ => CanConnectToHueBridge());
        
        StartEffectCommand = new RelayCommand(async _ => await StartEffectAsync(), _ => CanStartEffect());
        StopEffectCommand = new RelayCommand(async _ => await StopEffectAsync(), _ => CanStopEffect());
        
        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        OpenSettingsCommand = new RelayCommand(_ => OpenSettings());

        // Load initial data
        RefreshAudioDevices();
        LoadAvailableEffects();

        _logger.LogInformation("MainWindowViewModel initialized");
    }

    #region Properties

    public ObservableCollection<AudioDevice> AudioDevices
    {
        get => _audioDevices;
        set => SetProperty(ref _audioDevices, value);
    }

    public AudioDevice? SelectedAudioDevice
    {
        get => _selectedAudioDevice;
        set
        {
            if (SetProperty(ref _selectedAudioDevice, value) && value != null)
            {
                _audioService.SelectDevice(value!);
                _logger.LogInformation("Selected audio device: {DeviceName}", value.Name);
            }
        }
    }

    public bool IsAudioCapturing
    {
        get => _isAudioCapturing;
        set => SetProperty(ref _isAudioCapturing, value);
    }

    public ObservableCollection<HueBridge> HueBridges
    {
        get => _hueBridges;
        set => SetProperty(ref _hueBridges, value);
    }

    public HueBridge? SelectedHueBridge
    {
        get => _selectedHueBridge;
        set => SetProperty(ref _selectedHueBridge, value);
    }

    public ObservableCollection<HueLight> HueLights
    {
        get => _hueLights;
        set => SetProperty(ref _hueLights, value);
    }

    public bool IsHueConnected
    {
        get => _isHueConnected;
        set => SetProperty(ref _isHueConnected, value);
    }

    public ObservableCollection<string> AvailableEffects
    {
        get => _availableEffects;
        set => SetProperty(ref _availableEffects, value);
    }

    public string? SelectedEffect
    {
        get => _selectedEffect;
        set => SetProperty(ref _selectedEffect, value);
    }

    public bool IsEffectRunning
    {
        get => _isEffectRunning;
        set => SetProperty(ref _isEffectRunning, value);
    }

    public double Intensity
    {
        get => _intensity;
        set
        {
            if (SetProperty(ref _intensity, value))
            {
                UpdateEffectConfig();
            }
        }
    }

    public double Speed
    {
        get => _speed;
        set
        {
            if (SetProperty(ref _speed, value))
            {
                UpdateEffectConfig();
            }
        }
    }

    public double Brightness
    {
        get => _brightness;
        set
        {
            if (SetProperty(ref _brightness, value))
            {
                UpdateEffectConfig();
            }
        }
    }

    public double AudioSensitivity
    {
        get => _audioSensitivity;
        set
        {
            if (SetProperty(ref _audioSensitivity, value))
            {
                UpdateEffectConfig();
            }
        }
    }

    public bool AudioReactive
    {
        get => _audioReactive;
        set
        {
            if (SetProperty(ref _audioReactive, value))
            {
                UpdateEffectConfig();
            }
        }
    }

    public bool SmoothTransitions
    {
        get => _smoothTransitions;
        set
        {
            if (SetProperty(ref _smoothTransitions, value))
            {
                UpdateEffectConfig();
            }
        }
    }

    public double HueVariation
    {
        get => _hueVariation;
        set
        {
            if (SetProperty(ref _hueVariation, value))
            {
                UpdateEffectConfig();
            }
        }
    }

    public double Saturation
    {
        get => _saturation;
        set
        {
            if (SetProperty(ref _saturation, value))
            {
                UpdateEffectConfig();
            }
        }
    }

    public double ColorTemperature
    {
        get => _colorTemperature;
        set
        {
            if (SetProperty(ref _colorTemperature, value))
            {
                UpdateEffectConfig();
            }
        }
    }

    public float[] SpectralData
    {
        get => _spectralData;
        set => SetProperty(ref _spectralData, value);
    }

    public double CurrentBpm
    {
        get => _currentBpm;
        set => SetProperty(ref _currentBpm, value);
    }

    public bool IsBeatDetected
    {
        get => _isBeatDetected;
        set => SetProperty(ref _isBeatDetected, value);
    }

    public bool IsDarkTheme
    {
        get => _isDarkTheme;
        set => SetProperty(ref _isDarkTheme, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    #endregion

    #region Commands

    public ICommand RefreshAudioDevicesCommand { get; }
    public ICommand StartAudioCaptureCommand { get; }
    public ICommand StopAudioCaptureCommand { get; }
    public ICommand DiscoverHueBridgesCommand { get; }
    public ICommand ConnectToHueBridgeCommand { get; }
    public ICommand StartEffectCommand { get; }
    public ICommand StopEffectCommand { get; }
    public ICommand ToggleThemeCommand { get; }
    public ICommand OpenSettingsCommand { get; }

    #endregion

    #region Audio Methods

    private void RefreshAudioDevices()
    {
        try
        {
            var devices = _audioService.GetOutputDevices();
            AudioDevices = new ObservableCollection<AudioDevice>(devices);
            
            if (AudioDevices.Any() && SelectedAudioDevice == null)
            {
                SelectedAudioDevice = AudioDevices.First();
            }
            
            StatusMessage = $"Found {AudioDevices.Count} audio device(s)";
            _logger.LogInformation("Refreshed audio devices, found {Count}", AudioDevices.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing audio devices");
            StatusMessage = "Error loading audio devices";
        }
    }

    private bool CanStartAudioCapture() => !IsAudioCapturing && SelectedAudioDevice != null;

    private void StartAudioCapture()
    {
        try
        {
            _audioService.StartCapture();
            IsAudioCapturing = true;
            StatusMessage = "Audio capture started";
            _logger.LogInformation("Audio capture started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting audio capture");
            StatusMessage = "Error starting audio capture";
        }
    }

    private bool CanStopAudioCapture() => IsAudioCapturing;

    private void StopAudioCapture()
    {
        try
        {
            _audioService.StopCapture();
            IsAudioCapturing = false;
            StatusMessage = "Audio capture stopped";
            _logger.LogInformation("Audio capture stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping audio capture");
            StatusMessage = "Error stopping audio capture";
        }
    }

    private void OnAudioDataAvailable(object? sender, AudioDataEventArgs e)
    {
        // Forward to FFT processor
        _fftProcessor.ProcessAudio(e.Samples, e.SampleRate);
    }

    private void OnSpectralDataAvailable(object? sender, SpectralDataEventArgs e)
    {
        // Update visualizer data on UI thread
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            SpectralData = new float[] 
            { 
                (float)e.LowFrequencyEnergy, 
                (float)e.MidFrequencyEnergy, 
                (float)e.HighFrequencyEnergy 
            };
        });
    }

    private void OnBeatDetected(object? sender, BeatDetectedEventArgs e)
    {
        // Update beat indicator on UI thread
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            CurrentBpm = e.BPM;
            IsBeatDetected = true;
            
            // Reset beat indicator after a short delay
            Task.Delay(100).ContinueWith(_ =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    IsBeatDetected = false;
                });
            });
        });
    }

    #endregion

    #region Hue Methods

    private async Task DiscoverHueBridgesAsync()
    {
        try
        {
            StatusMessage = "Discovering Hue bridges...";
            var bridges = await _hueService.DiscoverBridgesAsync();
            HueBridges = new ObservableCollection<HueBridge>(bridges);
            
            if (HueBridges.Any() && SelectedHueBridge == null)
            {
                SelectedHueBridge = HueBridges.First();
            }
            
            StatusMessage = $"Found {HueBridges.Count} Hue bridge(s)";
            _logger.LogInformation("Discovered Hue bridges, found {Count}", HueBridges.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering Hue bridges");
            StatusMessage = "Error discovering Hue bridges";
        }
    }

    private bool CanConnectToHueBridge() => !IsHueConnected && SelectedHueBridge != null;

    private async Task ConnectToHueBridgeAsync()
    {
        if (SelectedHueBridge == null)
            return;

        try
        {
            StatusMessage = "Connecting to Hue bridge... Press the bridge button!";
            var result = await _hueService.RegisterAsync(
                SelectedHueBridge, 
                "LightJockey", 
                "Desktop");
            
            if (result.IsSuccess && !string.IsNullOrEmpty(result.AppKey))
            {
                await _hueService.ConnectAsync(SelectedHueBridge, result.AppKey);
                IsHueConnected = true;
                var lights = await _hueService.GetLightsAsync();
                HueLights = new ObservableCollection<HueLight>(lights);
                StatusMessage = $"Connected! Found {HueLights.Count} light(s)";
                _logger.LogInformation("Connected to Hue bridge, found {Count} lights", HueLights.Count);
            }
            else
            {
                StatusMessage = result.ErrorMessage ?? "Connection failed. Please press the bridge button and try again.";
                _logger.LogWarning("Hue bridge connection failed: {Error}", result.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to Hue bridge");
            StatusMessage = "Error connecting to Hue bridge";
        }
    }

    #endregion

    #region Effect Methods

    private void LoadAvailableEffects()
    {
        var effects = _effectEngine.GetAvailableEffects();
        AvailableEffects = new ObservableCollection<string>(effects);
        
        if (AvailableEffects.Any() && SelectedEffect == null)
        {
            SelectedEffect = AvailableEffects.First();
        }
        
        _logger.LogInformation("Loaded {Count} available effects", AvailableEffects.Count);
    }

    private bool CanStartEffect() => !IsEffectRunning && !string.IsNullOrEmpty(SelectedEffect) && IsHueConnected;

    private async Task StartEffectAsync()
    {
        if (string.IsNullOrEmpty(SelectedEffect))
            return;

        try
        {
            var config = CreateEffectConfig();
            var success = await _effectEngine.SetActiveEffectAsync(SelectedEffect, config);
            
            if (success)
            {
                IsEffectRunning = true;
                StatusMessage = $"Effect '{SelectedEffect}' started";
                _logger.LogInformation("Started effect: {EffectName}", SelectedEffect);
            }
            else
            {
                StatusMessage = "Failed to start effect";
                _logger.LogWarning("Failed to start effect: {EffectName}", SelectedEffect);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting effect");
            StatusMessage = "Error starting effect";
        }
    }

    private bool CanStopEffect() => IsEffectRunning;

    private async Task StopEffectAsync()
    {
        try
        {
            await _effectEngine.StopActiveEffectAsync();
            IsEffectRunning = false;
            StatusMessage = "Effect stopped";
            _logger.LogInformation("Effect stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping effect");
            StatusMessage = "Error stopping effect";
        }
    }

    private EffectConfig CreateEffectConfig()
    {
        return new EffectConfig
        {
            Intensity = Intensity,
            Speed = Speed,
            Brightness = Brightness,
            AudioReactive = AudioReactive,
            AudioSensitivity = AudioSensitivity,
            SmoothTransitions = SmoothTransitions,
            HueVariation = HueVariation,
            Saturation = Saturation,
            ColorTemperature = ColorTemperature
        };
    }

    private void UpdateEffectConfig()
    {
        if (IsEffectRunning)
        {
            var config = CreateEffectConfig();
            _effectEngine.UpdateActiveEffectConfig(config);
        }
    }

    private void OnActiveEffectChanged(object? sender, string? effectName)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            IsEffectRunning = effectName != null;
        });
    }

    private void OnEffectError(object? sender, string error)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            StatusMessage = $"Effect error: {error}";
            _logger.LogError("Effect error: {Error}", error);
        });
    }

    #endregion

    #region Theme Methods

    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        _logger.LogInformation("Theme changed to: {Theme}", IsDarkTheme ? "Dark" : "Light");
    }

    #endregion

    #region Settings Methods

    private void OpenSettings()
    {
        var settingsViewModel = new SettingsViewModel(_configurationService);
        var settingsWindow = new SettingsWindow(settingsViewModel)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };
        settingsWindow.ShowDialog();
    }

    #endregion

    public void Dispose()
    {
        if (_disposed)
            return;

        _logger.LogDebug("Disposing MainWindowViewModel");

        // Unsubscribe from events
        _audioService.AudioDataAvailable -= OnAudioDataAvailable;
        _effectEngine.ActiveEffectChanged -= OnActiveEffectChanged;
        _effectEngine.EffectError -= OnEffectError;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
