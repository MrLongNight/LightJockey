using System;
using System.ComponentModel;
using System.Windows.Input;
using LightJockey.Services;
using LightJockey.Utilities;
using Microsoft.Extensions.Logging;
using LightJockey.Models;
using System.Windows.Threading;

namespace LightJockey.ViewModels;

/// <summary>
/// ViewModel for the main window. Acts as an orchestrator for sub-ViewModels.
/// </summary>
public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IAudioService _audioService;
    private readonly IFFTProcessor _fftProcessor;
    private readonly IEffectEngine _effectEngine;
    private readonly IDialogService _dialogService;
    private readonly DispatcherTimer _beatIndicatorTimer;
    private bool _disposed;

    // Visualizer data
    private float[] _spectralData = Array.Empty<float>();
    private double _currentBpm;
    private bool _isBeatDetected;

    // Theme
    private bool _isDarkTheme = true;

    // Status messages
    private string _statusMessage = "Ready";

    public AudioControlViewModel AudioControlViewModel { get; }
    public HueControlViewModel HueControlViewModel { get; }
    public EffectControlViewModel EffectControlViewModel { get; }
    public MetricsViewModel MetricsViewModel { get; }

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IAudioService audioService,
        IFFTProcessor fftProcessor,
        ISpectralAnalyzer spectralAnalyzer,
        IBeatDetector beatDetector,
        IEffectEngine effectEngine,
        MetricsViewModel metricsViewModel,
        AudioControlViewModel audioControlViewModel,
        HueControlViewModel hueControlViewModel,
        EffectControlViewModel effectControlViewModel,
        IDialogService dialogService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
        _fftProcessor = fftProcessor ?? throw new ArgumentNullException(nameof(fftProcessor));
        _effectEngine = effectEngine ?? throw new ArgumentNullException(nameof(effectEngine));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        // Assign the sub-ViewModels
        MetricsViewModel = metricsViewModel ?? throw new ArgumentNullException(nameof(metricsViewModel));
        AudioControlViewModel = audioControlViewModel ?? throw new ArgumentNullException(nameof(audioControlViewModel));
        HueControlViewModel = hueControlViewModel ?? throw new ArgumentNullException(nameof(hueControlViewModel));
        EffectControlViewModel = effectControlViewModel ?? throw new ArgumentNullException(nameof(effectControlViewModel));

        // Initialize beat indicator timer
        _beatIndicatorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _beatIndicatorTimer.Tick += (s, e) =>
        {
            IsBeatDetected = false;
            _beatIndicatorTimer.Stop();
        };

        // Subscribe to events that affect this VM (visualizer, global status)
        _audioService.AudioDataAvailable += OnAudioDataAvailable;
        spectralAnalyzer.SpectralDataAvailable += OnSpectralDataAvailable;
        beatDetector.BeatDetected += OnBeatDetected;
        _effectEngine.ActiveEffectChanged += OnActiveEffectChanged;
        _effectEngine.EffectError += OnEffectError;
        
        // Aggregate status messages from sub-ViewModels
        AudioControlViewModel.PropertyChanged += OnSubViewModelPropertyChanged;
        HueControlViewModel.PropertyChanged += OnSubViewModelPropertyChanged;
        EffectControlViewModel.PropertyChanged += OnSubViewModelPropertyChanged;

        // Initialize commands that belong to the main window
        ToggleThemeCommand = new RelayCommand(_ => ToggleTheme());
        OpenSettingsCommand = new RelayCommand(OpenSettings);

        _logger.LogInformation("MainWindowViewModel initialized");
    }

    #region Properties

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

    public ICommand ToggleThemeCommand { get; }
    public ICommand OpenSettingsCommand { get; }

    #endregion

    #region Event Handlers & Methods

    private void OnAudioDataAvailable(object? sender, AudioDataEventArgs e)
    {
        // Forward to FFT processor for analysis chain
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
            _beatIndicatorTimer.Start(); // (Re)start the timer
        });
    }

    private void OnActiveEffectChanged(object? sender, string? effectName)
    {
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            EffectControlViewModel.IsEffectRunning = effectName != null;
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

    private void OnSubViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(StatusMessage)) return;

        switch (sender)
        {
            case AudioControlViewModel audioVm when !string.IsNullOrWhiteSpace(audioVm.StatusMessage):
                StatusMessage = audioVm.StatusMessage;
                break;
            case HueControlViewModel hueVm when !string.IsNullOrWhiteSpace(hueVm.StatusMessage):
                StatusMessage = hueVm.StatusMessage;
                break;
            case EffectControlViewModel effectVm when !string.IsNullOrWhiteSpace(effectVm.StatusMessage):
                StatusMessage = effectVm.StatusMessage;
                break;
        }
    }

    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        _logger.LogInformation("Theme changed to: {Theme}", IsDarkTheme ? "Dark" : "Light");
    }

    private void OpenSettings(object? obj)
    {
        _dialogService.ShowSettings();
    }

    #endregion

    public void Dispose()
    {
        if (_disposed) return;

        _logger.LogDebug("Disposing MainWindowViewModel");

        _beatIndicatorTimer.Stop();

        // Unsubscribe from events
        _audioService.AudioDataAvailable -= OnAudioDataAvailable;
        _effectEngine.ActiveEffectChanged -= OnActiveEffectChanged;
        _effectEngine.EffectError -= OnEffectError;

        AudioControlViewModel.PropertyChanged -= OnSubViewModelPropertyChanged;
        HueControlViewModel.PropertyChanged -= OnSubViewModelPropertyChanged;
        EffectControlViewModel.PropertyChanged -= OnSubViewModelPropertyChanged;

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
