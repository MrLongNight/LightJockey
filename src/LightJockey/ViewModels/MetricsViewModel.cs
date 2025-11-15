using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using LightJockey.Models;
using LightJockey.Services;
using LightJockey.Utilities;
using Microsoft.Extensions.Logging;

namespace LightJockey.ViewModels
{
    public partial class MetricsViewModel : ViewModelBase
    {
        private readonly IMetricsService _metricsService;
        private readonly ILogger<MetricsViewModel> _logger;
        private string _exportStatusMessage;

        [ObservableProperty]
        private PerformanceMetrics _performanceMetrics;

        public string ExportStatusMessage
        {
            get => _exportStatusMessage;
            set => SetProperty(ref _exportStatusMessage, value);
        }

        public ICommand ExportMetricsCommand { get; }

        public MetricsViewModel(IMetricsService metricsService, ILogger<MetricsViewModel> logger)
        {
            _metricsService = metricsService;
            _logger = logger;
            _performanceMetrics = new PerformanceMetrics();
            _exportStatusMessage = string.Empty;

            ExportMetricsCommand = new RelayCommand(ExportMetrics);
            _metricsService.MetricsUpdated += OnMetricsUpdated;
        }

        private void OnMetricsUpdated(object? sender, PerformanceMetrics e)
        {
            PerformanceMetrics = e;
        }

        private void ExportMetrics(object? parameter)
        {
            try
            {
                var path = "metrics.csv";
                _metricsService.ExportMetrics(path);
                ExportStatusMessage = $"Metrics exported to {path}";
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error exporting metrics");
                ExportStatusMessage = "Error exporting metrics";
            }
        }
    }
}
