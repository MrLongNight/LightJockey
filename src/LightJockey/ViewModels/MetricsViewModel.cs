using CommunityToolkit.Mvvm.Input;
using LightJockey.Models;
using LightJockey.Services;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace LightJockey.ViewModels
{
    public class MetricsViewModel : ViewModelBase
    {
        private readonly IMetricsService _metricsService;
        private readonly ILogger<MetricsViewModel> _logger;
        private readonly DispatcherTimer _timer;
        private string _exportStatusMessage = string.Empty;

        public ObservableCollection<PerformanceMetrics> MetricsHistory { get; } = new();

        public IAsyncRelayCommand ExportMetricsCommand { get; }

        public string ExportStatusMessage
        {
            get => _exportStatusMessage;
            set => SetProperty(ref _exportStatusMessage, value);
        }

        public MetricsViewModel(IMetricsService metricsService, ILogger<MetricsViewModel> logger)
        {
            _metricsService = metricsService;
            _logger = logger;

            ExportMetricsCommand = new AsyncRelayCommand(ExportMetricsAsync);

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += (s, e) => UpdateMetrics();
            _timer.Start();
        }

        private void UpdateMetrics()
        {
            MetricsHistory.Clear();
            foreach (var metric in _metricsService.GetMetricsHistory())
            {
                MetricsHistory.Add(metric);
            }
        }

        private async Task ExportMetricsAsync()
        {
            _logger.LogInformation("Exporting metrics...");
            ExportStatusMessage = "Exporting...";
            try
            {
                var filePath = await _metricsService.ExportMetricsToCsvAsync();
                ExportStatusMessage = $"Metrics exported to {filePath}";
                _logger.LogInformation("Metrics exported successfully to {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                ExportStatusMessage = "Export failed. Check logs for details.";
                _logger.LogError(ex, "Failed to export metrics");
            }
        }
    }
}
