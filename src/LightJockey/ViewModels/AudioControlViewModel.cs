using LightJockey.Services;
using LightJockey.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;

namespace LightJockey.ViewModels
{
    public class AudioControlViewModel : ViewModelBase
    {
        private readonly ILogger<AudioControlViewModel> _logger;
        private readonly IAudioService _audioService;

        private ObservableCollection<AudioDevice> _audioDevices = new();
        private AudioDevice? _selectedAudioDevice;
        private bool _isAudioCapturing;
        private string _statusMessage = "Ready";

        public AudioControlViewModel(ILogger<AudioControlViewModel> logger, IAudioService audioService)
        {
            _logger = logger;
            _audioService = audioService;

            RefreshAudioDevicesCommand = new RelayCommand<object?>(_ => RefreshAudioDevices());
            StartAudioCaptureCommand = new RelayCommand<object?>(_ => StartAudioCapture(), _ => CanStartAudioCapture());
            StopAudioCaptureCommand = new RelayCommand<object?>(_ => StopAudioCapture(), _ => CanStopAudioCapture());

            RefreshAudioDevices();
        }

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
                    // Re-evaluate command states when selection changes
                    StartAudioCaptureCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public bool IsAudioCapturing
        {
            get => _isAudioCapturing;
            set
            {
                if (SetProperty(ref _isAudioCapturing, value))
                {
                    // Update command states when capture status changes
                    StartAudioCaptureCommand.NotifyCanExecuteChanged();
                    StopAudioCaptureCommand.NotifyCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public IRelayCommand RefreshAudioDevicesCommand { get; }
        public IRelayCommand StartAudioCaptureCommand { get; }
        public IRelayCommand StopAudioCaptureCommand { get; }

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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error stopping audio capture");
                StatusMessage = "Error stopping audio capture";
            }
        }
    }
}
