using LightJockey.Services;
using LightJockey.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using LightJockey.Utilities;
using System;
using System.Windows.Threading;

namespace LightJockey.ViewModels
{
    public class EffectControlViewModel : ViewModelBase
    {
        private readonly ILogger<EffectControlViewModel> _logger;
        private readonly IEffectEngine _effectEngine;
        private readonly HueControlViewModel _hueControlViewModel; // Dependency on Hue state
        private readonly DispatcherTimer _debounceTimer;

        private ObservableCollection<string> _availableEffects = new();
        private string? _selectedEffect;
        private bool _isEffectRunning;
        private string _statusMessage = "Ready";

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

        public EffectControlViewModel(ILogger<EffectControlViewModel> logger, IEffectEngine effectEngine, HueControlViewModel hueControlViewModel)
        {
            _logger = logger;
            _effectEngine = effectEngine;
            _hueControlViewModel = hueControlViewModel;

            StartEffectCommand = new RelayCommand(async _ => await StartEffectAsync(), _ => CanStartEffect());
            StopEffectCommand = new RelayCommand(async _ => await StopEffectAsync(), _ => CanStopEffect());

            _hueControlViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(HueControlViewModel.IsHueConnected))
                {
                    ((RelayCommand)StartEffectCommand).NotifyCanExecuteChanged();
                }
            };

            // Initialize debounce timer
            _debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            _debounceTimer.Tick += (s, e) =>
            {
                _debounceTimer.Stop();
                UpdateEffectConfig();
            };

            LoadAvailableEffects();
        }

        public ObservableCollection<string> AvailableEffects
        {
            get => _availableEffects;
            set => SetProperty(ref _availableEffects, value);
        }

        public string? SelectedEffect
        {
            get => _selectedEffect;
            set
            {
                if (SetProperty(ref _selectedEffect, value))
                {
                    ((RelayCommand)StartEffectCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public bool IsEffectRunning
        {
            get => _isEffectRunning;
            set
            {
                if (SetProperty(ref _isEffectRunning, value))
                {
                    ((RelayCommand)StartEffectCommand).NotifyCanExecuteChanged();
                    ((RelayCommand)StopEffectCommand).NotifyCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        #region Effect Parameters Properties
        public double Intensity { get => _intensity; set => SetProperty(ref _intensity, value, DebounceUpdate); }
        public double Speed { get => _speed; set => SetProperty(ref _speed, value, DebounceUpdate); }
        public double Brightness { get => _brightness; set => SetProperty(ref _brightness, value, DebounceUpdate); }
        public double AudioSensitivity { get => _audioSensitivity; set => SetProperty(ref _audioSensitivity, value, DebounceUpdate); }
        public bool AudioReactive { get => _audioReactive; set => SetProperty(ref _audioReactive, value, DebounceUpdate); }
        public bool SmoothTransitions { get => _smoothTransitions; set => SetProperty(ref _smoothTransitions, value, DebounceUpdate); }
        public double HueVariation { get => _hueVariation; set => SetProperty(ref _hueVariation, value, DebounceUpdate); }
        public double Saturation { get => _saturation; set => SetProperty(ref _saturation, value, DebounceUpdate); }
        public double ColorTemperature { get => _colorTemperature; set => SetProperty(ref _colorTemperature, value, DebounceUpdate); }
        #endregion

        public ICommand StartEffectCommand { get; }
        public ICommand StopEffectCommand { get; }

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

        private bool CanStartEffect() => !IsEffectRunning && !string.IsNullOrEmpty(SelectedEffect) && _hueControlViewModel.IsHueConnected;

        private async Task StartEffectAsync()
        {
            if (string.IsNullOrEmpty(SelectedEffect)) return;

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

        private void DebounceUpdate()
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
        }

        private void UpdateEffectConfig()
        {
            if (IsEffectRunning)
            {
                var config = CreateEffectConfig();
                _effectEngine.UpdateActiveEffectConfig(config);
                _logger.LogTrace("Effect configuration updated");
            }
        }

        // Helper for property setters
        private void SetProperty<T>(ref T storage, T value, Action? onChanged)
        {
            if (SetProperty(ref storage, value))
            {
                onChanged?.Invoke();
            }
        }
    }
}
