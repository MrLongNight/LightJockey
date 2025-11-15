using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LightJockey.Services;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharp;
using Microsoft.Extensions.Logging;

namespace LightJockey.ViewModels
{
    public class MetricsViewModel : ObservableObject
    {
        private readonly IMetricsService _metricsService;
        private readonly ILogger<MetricsViewModel> _logger;
        private readonly DispatcherTimer _timer;

        public ObservableCollection<ISeries> Series { get; set; }
        public ObservableCollection<string> Labels { get; set; }

        public MetricsViewModel(IMetricsService metricsService, ILogger<MetricsViewModel> logger)
        {
            _metricsService = metricsService;
            _logger = logger;

            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<double> { Values = new ObservableCollection<double>(), Name = "FPS" },
                new LineSeries<double> { Values = new ObservableCollection<double>(), Name = "Latency (ms)" }
            };
            Labels = new ObservableCollection<string>();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => UpdateChart();
            _timer.Start();
        }

        private void UpdateChart()
        {
            var history = _metricsService.MetricsHistory;
            if (history.Count == 0) return;

            var fpsValues = (ObservableCollection<double>)Series[0].Values;
            var latencyValues = (ObservableCollection<double>)Series[1].Values;

            fpsValues.Clear();
            latencyValues.Clear();
            Labels.Clear();

            foreach (var metrics in history)
            {
                fpsValues.Add(metrics.StreamingFPS);
                latencyValues.Add(metrics.TotalLatencyMs);
                Labels.Add(metrics.Timestamp.ToString("HH:mm:ss"));
            }
        }
    }
}
