using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LightJockey.ViewModels
{
    public partial class SettingsViewModel : ViewModelBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<SettingsViewModel> _logger;
        private AppSettings _appSettings;

        [ObservableProperty]
        private string _selectedLogLevel;

        [ObservableProperty]
        private int _retainedLogFileCount;

        public List<string> LogLevels { get; }

        public IAsyncRelayCommand SaveCommand { get; }

        public SettingsViewModel(IConfigurationService configurationService, ILogger<SettingsViewModel> logger)
        {
            _configurationService = configurationService;
            _logger = logger;
            _appSettings = new AppSettings();
            _selectedLogLevel = _appSettings.LogLevel;
            _retainedLogFileCount = _appSettings.RetainedLogFileCount;
            LogLevels = Enum.GetNames(typeof(Serilog.Events.LogEventLevel)).ToList();
            SaveCommand = new AsyncRelayCommand(SaveSettings);
            Task.Run(LoadSettings);
        }

        private async Task LoadSettings()
        {
            _appSettings = await _configurationService.LoadAppSettingsAsync() ?? new AppSettings();
            SelectedLogLevel = _appSettings.LogLevel;
            RetainedLogFileCount = _appSettings.RetainedLogFileCount;
        }

        private async Task SaveSettings()
        {
            _appSettings.LogLevel = SelectedLogLevel;
            _appSettings.RetainedLogFileCount = RetainedLogFileCount;
            await _configurationService.SaveAppSettingsAsync(_appSettings);
            _logger.LogInformation("Application settings saved.");
        }
    }
}
