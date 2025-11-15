using LightJockey.ViewModels;
using LightJockey.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LightJockey.Services
{
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;

        public DialogService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void ShowSettings()
        {
            // Resolve the ViewModel from the DI container
            var settingsViewModel = _serviceProvider.GetRequiredService<SettingsViewModel>();

            var settingsWindow = new SettingsWindow(settingsViewModel)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            settingsWindow.ShowDialog();
        }
    }
}
