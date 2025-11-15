using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LightJockey.Services;
using LightJockey.Models;

namespace LightJockey.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IConfigurationService _configurationService;

    [ObservableProperty]
    private AppSettings _appSettings;

    public SettingsViewModel(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        _appSettings = _configurationService.LoadAppSettings();

        SaveSettingsCommand = new RelayCommand(SaveSettings);
    }

    public IRelayCommand SaveSettingsCommand { get; }

    private void SaveSettings()
    {
        _configurationService.SaveAppSettings(AppSettings);
    }
}
