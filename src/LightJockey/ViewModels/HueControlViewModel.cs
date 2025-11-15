using LightJockey.Services;
using LightJockey.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Threading.Tasks;
using LightJockey.Utilities;

namespace LightJockey.ViewModels
{
    public class HueControlViewModel : ViewModelBase
    {
        private readonly ILogger<HueControlViewModel> _logger;
        private readonly IHueService _hueService;

        private ObservableCollection<HueBridge> _hueBridges = new();
        private HueBridge? _selectedHueBridge;
        private ObservableCollection<HueLight> _hueLights = new();
        private bool _isHueConnected;
        private string _statusMessage = "Ready";

        public HueControlViewModel(ILogger<HueControlViewModel> logger, IHueService hueService)
        {
            _logger = logger;
            _hueService = hueService;

            DiscoverHueBridgesCommand = new RelayCommand(async _ => await DiscoverHueBridgesAsync());
            ConnectToHueBridgeCommand = new RelayCommand(
                async obj => await ConnectToHueBridgeAsync(),
                obj => CanConnectToHueBridge()
            );
        }

        public ObservableCollection<HueBridge> HueBridges
        {
            get => _hueBridges;
            set => SetProperty(ref _hueBridges, value);
        }

        public HueBridge? SelectedHueBridge
        {
            get => _selectedHueBridge;
            set
            {
                if (SetProperty(ref _selectedHueBridge, value))
                {
                    ((RelayCommand)ConnectToHueBridgeCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<HueLight> HueLights
        {
            get => _hueLights;
            set => SetProperty(ref _hueLights, value);
        }

        public bool IsHueConnected
        {
            get => _isHueConnected;
            set
            {
                if (SetProperty(ref _isHueConnected, value))
                {
                    ((RelayCommand)ConnectToHueBridgeCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public ICommand DiscoverHueBridgesCommand { get; }
        public ICommand ConnectToHueBridgeCommand { get; }

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
            catch (System.Exception ex)
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
                var result = await _hueService.RegisterAsync(SelectedHueBridge, "LightJockey", "Desktop");

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
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error connecting to Hue bridge");
                StatusMessage = "Error connecting to Hue bridge";
            }
        }
    }
}
