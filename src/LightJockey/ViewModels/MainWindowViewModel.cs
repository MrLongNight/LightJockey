using System;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using LightJockey.Models;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;
using System.Windows.Threading;
using System.Collections.ObjectModel;

namespace LightJockey.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IAudioService _audioService;
    private readonly IFFTProcessor _fftProcessor;
    private readonly IEffectEngine _effectEngine;
    private readonly IDialogService _dialogService;
    private readonly DispatcherTimer _beatIndicatorTimer;
    private bool _disposed;

    private float[] _spectralData = Array.Empty<float>();
    private double _currentBpm;
    private bool _isBeatDetected;
    private bool _isDarkTheme = true;
    private string _statusMessage = "Ready";

    // Throttling für UI Updates
    private DateTime _lastSpectralUpdate = DateTime.MinValue;

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
        _logger = logger;
        _audioService = audioService;
        _fftProcessor = fftProcessor;
        _effectEngine = effectEngine;
        _dialogService = dialogService;

        MetricsViewModel = metricsViewModel;
        AudioControlViewModel = audioControlViewModel;
        HueControlViewModel = hueControlViewModel;
        EffectControlViewModel = effectControlViewModel;

        _beatIndicatorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _beatIndicatorTimer.Tick += (s, e) => { IsBeatDetected = false; _beatIndicatorTimer.Stop(); };

        _audioService.AudioDataAvailable += OnAudioDataAvailable;
        spectralAnalyzer.SpectralDataAvailable += OnSpectralDataAvailable;
        beatDetector.BeatDetected += OnBeatDetected;
        _effectEngine.ActiveEffectChanged += OnActiveEffectChanged;
        _effectEngine.EffectError += OnEffectError;

        AudioControlViewModel.PropertyChanged += OnSubViewModelPropertyChanged;
        HueControlViewModel.PropertyChanged += OnSubViewModelPropertyChanged;
        EffectControlViewModel.PropertyChanged += OnSubViewModelPropertyChanged;

        ToggleThemeCommand = new RelayCommand(ToggleTheme);
        OpenSettingsCommand = new RelayCommand(() => OpenSettings(null));
    }

    #region Properties for Tests
    public ObservableCollection<AudioDevice> AudioDevices => AudioControlViewModel.AudioDevices;
    public AudioDevice? SelectedAudioDevice { get => AudioControlViewModel.SelectedAudioDevice; set => AudioControlViewModel.SelectedAudioDevice = value; }
    public bool IsAudioCapturing { get => AudioControlViewModel.IsAudioCapturing; set => AudioControlViewModel.IsAudioCapturing = value; }
    public ObservableCollection<HueBridge> HueBridges => HueControlViewModel.HueBridges;
    public ObservableCollection<HueLight> HueLights => HueControlViewModel.HueLights;
    public bool IsHueConnected { get => HueControlViewModel.IsHueConnected; set => HueControlViewModel.IsHueConnected = value; }
    public ObservableCollection<string> AvailableEffects => EffectControlViewModel.AvailableEffects;
    public string? SelectedEffect { get => EffectControlViewModel.SelectedEffect; set => EffectControlViewModel.SelectedEffect = value; }
    public bool IsEffectRunning { get => EffectControlViewModel.IsEffectRunning; set => EffectControlViewModel.IsEffectRunning = value; }
    public double Intensity { get => EffectControlViewModel.Intensity; set => EffectControlViewModel.Intensity = value; }
    public double Speed { get => EffectControlViewModel.Speed; set => EffectControlViewModel.Speed = value; }
    public double Brightness { get => EffectControlViewModel.Brightness; set => EffectControlViewModel.Brightness = value; }
    public double AudioSensitivity { get => EffectControlViewModel.AudioSensitivity; set => EffectControlViewModel.AudioSensitivity = value; }
    public bool AudioReactive { get => EffectControlViewModel.AudioReactive; set => EffectControlViewModel.AudioReactive = value; }
    public bool SmoothTransitions { get => EffectControlViewModel.SmoothTransitions; set => EffectControlViewModel.SmoothTransitions = value; }
    public double HueVariation { get => EffectControlViewModel.HueVariation; set => EffectControlViewModel.HueVariation = value; }
    public double Saturation { get => EffectControlViewModel.Saturation; set => EffectControlViewModel.Saturation = value; }
    public double ColorTemperature { get => EffectControlViewModel.ColorTemperature; set => EffectControlViewModel.ColorTemperature = value; }
    #endregion

    #region Commands for Tests
    public ICommand RefreshAudioDevicesCommand => AudioControlViewModel.RefreshAudioDevicesCommand;
    public ICommand StartAudioCaptureCommand => AudioControlViewModel.StartAudioCaptureCommand;
    public ICommand StopAudioCaptureCommand => AudioControlViewModel.StopAudioCaptureCommand;
    public ICommand DiscoverHueBridgesCommand => HueControlViewModel.DiscoverHueBridgesCommand;
    public ICommand ConnectToHueBridgeCommand => HueControlViewModel.ConnectToHueBridgeCommand;
    public ICommand StartEffectCommand => EffectControlViewModel.StartEffectCommand;
    public ICommand StopEffectCommand => EffectControlViewModel.StopEffectCommand;
    #endregion

    public float[] SpectralData { get => _spectralData; set => SetProperty(ref _spectralData, value); }
    public double CurrentBpm { get => _currentBpm; set => SetProperty(ref _currentBpm, value); }
    public bool IsBeatDetected { get => _isBeatDetected; set => SetProperty(ref _isBeatDetected, value); }
    public bool IsDarkTheme { get => _isDarkTheme; set => SetProperty(ref _isDarkTheme, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

    public ICommand ToggleThemeCommand { get; }
    public ICommand OpenSettingsCommand { get; }

    private void OnAudioDataAvailable(object? sender, AudioDataEventArgs e) => _fftProcessor.ProcessAudio(e.Samples, e.SampleRate);
    
    private void OnSpectralDataAvailable(object? sender, SpectralDataEventArgs e) 
    {
        // Throttling: Update UI max every 30ms to prevent freezing
        var now = DateTime.Now;
        if ((now - _lastSpectralUpdate).TotalMilliseconds < 30)
        {
            return;
        }
        _lastSpectralUpdate = now;

        System.Windows.Application.Current?.Dispatcher.Invoke(() => 
        { 
            SpectralData = new float[] { (float)e.LowFrequencyEnergy, (float)e.MidFrequencyEnergy, (float)e.HighFrequencyEnergy }; 
        });
    }

    private void OnBeatDetected(object? sender, BeatDetectedEventArgs e) => System.Windows.Application.Current?.Dispatcher.Invoke(() => { CurrentBpm = e.BPM; IsBeatDetected = true; _beatIndicatorTimer.Start(); });
    private void OnActiveEffectChanged(object? sender, string? effectName) => System.Windows.Application.Current?.Dispatcher.Invoke(() => { EffectControlViewModel.IsEffectRunning = effectName != null; });
    private void OnEffectError(object? sender, string error) => System.Windows.Application.Current?.Dispatcher.Invoke(() => { StatusMessage = $"Effect error: {error}"; _logger.LogError("Effect error: {Error}", error); });

    private void OnSubViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(StatusMessage)) return;
        if (sender is AudioControlViewModel audioVm && !string.IsNullOrWhiteSpace(audioVm.StatusMessage)) StatusMessage = audioVm.StatusMessage;
        else if (sender is HueControlViewModel hueVm && !string.IsNullOrWhiteSpace(hueVm.StatusMessage)) StatusMessage = hueVm.StatusMessage;
        else if (sender is EffectControlViewModel effectVm && !string.IsNullOrWhiteSpace(effectVm.StatusMessage)) StatusMessage = effectVm.StatusMessage;
    }

    private void ToggleTheme() => IsDarkTheme = !IsDarkTheme;
    private void OpenSettings(object? obj) => _dialogService.ShowSettings();

    public void Dispose()
    {
        if (_disposed) return;
        _beatIndicatorTimer.Stop();
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
