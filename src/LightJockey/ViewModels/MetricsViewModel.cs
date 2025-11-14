using LightJockey.Models;
using LightJockey.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace LightJockey.ViewModels
{
    public class MetricsViewModel : ViewModelBase
    {
        private readonly IMetricsService _metricsService;
        private readonly DispatcherTimer _timer;

        public ObservableCollection<PerformanceMetrics> MetricsHistory { get; } = new();

        public MetricsViewModel(IMetricsService metricsService)
        {
            _metricsService = metricsService;
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
    }
}
